/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "stdafx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "MPUrlSourceSplitter_Protocol_Http.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

#include <WinInet.h>
#include <stdio.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Httpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Http"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Http(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Http *protocol = (CMPUrlSourceSplitter_Protocol_Http *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Http::CMPUrlSourceSplitter_Protocol_Http(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->currentCookies = NULL;
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->receiveDataTimeout = HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamLength = 0;
  this->connectionState = None;
  this->mediaPackets = NULL;
  this->cacheFile = NULL;
  this->startStreamPosition = 0;
  this->endStreamPosition = 0;
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->mediaPackets = new CMediaPacketCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mediaPackets, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    version = CCurlInstance::GetCurlVersion();
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_Http::~CMPUrlSourceSplitter_Protocol_Http()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->currentCookies);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->mediaPackets);

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  if (this->lockCurlMutex != NULL)
  {
    CloseHandle(this->lockCurlMutex);
    this->lockCurlMutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Http::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  this->ClearSession();

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CProtocolPluginConfiguration *protocolConfiguration = new CProtocolPluginConfiguration(&result, (CParameterCollection *)parameters);
    CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->Initialize(protocolConfiguration));
    FREE_MEM_CLASS(protocolConfiguration);
  }

  const wchar_t *url = this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);
  CHECK_POINTER_HRESULT(result, url, result, E_URL_NOT_SPECIFIED);

  if (SUCCEEDED(result))
  {
    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    if (urlComponents == NULL)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'url components'");
      result = E_OUTOFMEMORY;
    }

    if (SUCCEEDED(result))
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      this->logger->Log(LOGGER_INFO, L"%s: %s: url: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);

      if (!InternetCrackUrl(url, 0, 0, urlComponents))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: InternetCrackUrl() error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
        result = E_FAIL;
      }
    }

    if (SUCCEEDED(result))
    {
      int length = urlComponents->dwSchemeLength + 1;
      ALLOC_MEM_DEFINE_SET(protocol, wchar_t, length, 0);
      if (protocol == NULL) 
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'protocol'");
        result = E_OUTOFMEMORY;
      }

      if (SUCCEEDED(result))
      {
        wcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);

        bool supportedProtocol = false;
        for (int i = 0; i < TOTAL_SUPPORTED_PROTOCOLS; i++)
        {
          if (_wcsnicmp(urlComponents->lpszScheme, SUPPORTED_PROTOCOLS[i], urlComponents->dwSchemeLength) == 0)
          {
            supportedProtocol = true;
            break;
          }
        }

        if (!supportedProtocol)
        {
          // not supported protocol
          this->logger->Log(LOGGER_INFO, L"%s: %s: unsupported protocol '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
          result = E_FAIL;
        }
      }
      FREE_MEM(protocol);
    }

    FREE_MEM(urlComponents);
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->connectionState == Opened))
    {
      {
        CLockMutex lockData(this->lockCurlMutex, INFINITE);

        FREE_MEM_CLASS(this->currentCookies);
        this->currentCookies = this->mainCurlInstance->GetCurrentCookies();

        unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
        if (bytesRead > 0)
        {
          this->lastReceiveDataTime = GetTickCount();

          CMediaPacket *mediaPacket = new CMediaPacket(&result);
          CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, mediaPacket->GetBuffer()->InitializeBuffer(bytesRead), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            mediaPacket->SetStart(this->currentStreamPosition);
            mediaPacket->SetEnd(this->currentStreamPosition + bytesRead - 1);
            mediaPacket->SetLoadedToMemoryTime(GetTickCount());

            mediaPacket->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData(), 0, bytesRead);
          }

          CHECK_CONDITION_HRESULT(result, this->mediaPackets->Add(mediaPacket), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaPacket));

          if (SUCCEEDED(result))
          {
            this->currentStreamPosition += (int64_t)bytesRead;
            this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
          }

          if (SUCCEEDED(result) && (!this->IsSetStreamLength()))
          {
            double streamSize = this->mainCurlInstance->GetDownloadContentLength();
            if (streamSize > 0)
            {
              LONGLONG total = LONGLONG(streamSize);
              this->streamLength = total;
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, total);
              
              this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
              this->flags &= ~(PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED | PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED);
            }
            else
            {
              // probably live stream, content length is not specified
              this->flags |= PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;

              if (this->streamLength == 0)
              {
                // stream length not set
                // just make guess
                this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
              }
              else if ((this->currentStreamPosition > (this->streamLength * 3 / 4)))
              {
                // it is time to adjust stream length, we are approaching to end but still we don't know total length
                this->streamLength = this->currentStreamPosition * 2;
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
              }
            }
          }
        }
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->connectionState == Opening))
    {
      // check HTTP response code
      long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();

      if (responseCode != 0)
      {
        // response code 200 - 299 = OK
        // response code 300 - 399 = redirect (OK)
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: HTTP status code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);

        if ((responseCode < 200) || (responseCode >= 400))
        {
          FREE_MEM_CLASS(this->mainCurlInstance);
          this->connectionState = None;
        }
        else
        {
          if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_SEEKING_SUPPORT_DETECTION))
          {
            this->flags |= (responseCode == 206) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
            this->flags |= (this->mainCurlInstance->GetHttpDownloadResponse()->GetRangesSupported()) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
          }

          this->flags |= (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SEEKING_SUPPORTED, true, HTTP_SEEKING_SUPPORTED_DEFAULT)) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;

          this->connectionState = Opened;
        }
      }
    }

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      // check end of stream or error on HTTP connection

      if (SUCCEEDED(this->mainCurlInstance->GetHttpDownloadResponse()->GetResultError()))
      {
        if (!this->IsLiveStreamDetected())
        {
          // check if all data removed from CURL instance
          if (this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
          {
            // check if we are not missing any data

            if (this->IsSetStreamLength() && (this->currentStreamPosition >= this->streamLength))
            {
              // stream length is set and our position is at least stream length, we reached end of stream
              this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
            }

            int64_t gapStartPosition = 0;
            int64_t gapEndPosition = 0;
            bool foundGap = false;

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: searching for gap in media packets, media packet count: %u, stream position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mediaPackets->Count(), this->currentStreamPosition);
            foundGap |= this->mediaPackets->FindGapInMediaPackets(this->currentStreamPosition, &gapStartPosition, &gapEndPosition);

            if (!foundGap)
            {
              // get last media packet to get total length
              CMediaPacket *lastMediaPacket = (this->mediaPackets->Count() != 0) ? this->mediaPackets->GetItem(this->mediaPackets->Count() - 1) : NULL;
              int64_t mediaPacketStreamLength = (lastMediaPacket != NULL) ? (lastMediaPacket->GetEnd() + 1) : 0;

              // in case of estimated total length just continue on position after last media packet
              // in case of not estimated total length, compare total length with last media packet
              if (this->IsStreamLengthEstimated() ||
                ((!this->IsStreamLengthEstimated()) && (mediaPacketStreamLength < this->streamLength)))
              {
                foundGap = true;
                gapStartPosition = mediaPacketStreamLength;
                gapEndPosition = mediaPacketStreamLength;
              }

              if (!foundGap)
              {
                this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"searching for gap in media packets from beginning");

                // we didn't find gap after current stream position, check from beggining of media packets
                foundGap |= this->mediaPackets->FindGapInMediaPackets(0, &gapStartPosition, &gapEndPosition);

                if (!foundGap)
                {
                  // we didn't find gap after beggining of media packets, we have whole stream

                  // whole stream downloaded
                  this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
                  this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"no gap found, all data received");

                  if (!this->IsSetStreamLength())
                  {
                    // get last media packet to get total length
                    CMediaPacket *mediaPacket = (this->mediaPackets->Count() != 0) ? this->mediaPackets->GetItem(this->mediaPackets->Count() - 1) : NULL;

                    this->streamLength = (mediaPacket != NULL) ? (mediaPacket->GetEnd() + 1) : 0;
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

                    this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
                    this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
                  }

                  // set current stream position to stream length to get correct result in QueryStreamProgress() method
                  this->currentStreamPosition = this->streamLength;

                  FREE_MEM_CLASS(this->mainCurlInstance);
                }
              }
            }

            if (foundGap)
            {
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found gap between or after media packets, start: %lld, end: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, gapStartPosition, gapEndPosition);

              // stops receiving data
              // this clear CURL instance and buffer, it leads to GetConnectionState() to PROTOCOL_CONNECTION_STATE_NONE result and connection will be reopened by ProtocolHoster
              this->StopReceivingData();

              // re-open connection at gap position
              this->startStreamPosition = gapStartPosition;
              this->endStreamPosition = gapEndPosition;
            }
          }
        }
        else
        {
          // whole stream downloaded
          this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
          this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"live stream, all data received");

          if (!this->IsSetStreamLength())
          {
            // get last media packet to get total length
            CMediaPacket *mediaPacket = (this->mediaPackets->Count() != 0) ? this->mediaPackets->GetItem(this->mediaPackets->Count() - 1) : NULL;

            this->streamLength = (mediaPacket != NULL) ? (mediaPacket->GetEnd() + 1) : 0;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

            this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
            this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
          }

          // set current stream position to stream length to get correct result in QueryStreamProgress() method
          this->currentStreamPosition = this->streamLength;

          FREE_MEM_CLASS(this->mainCurlInstance);
        }
      }
      else
      {
        // error while receiving data, stops receiving data
        // this clear CURL instance and buffer, it leads to GetConnectionState() to PROTOCOL_CONNECTION_STATE_NONE result and connection will be reopened by ProtocolHoster
        this->StopReceivingData();

        // re-open connection at last known position
        this->startStreamPosition = this->currentStreamPosition;
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL) && (!this->IsWholeStreamDownloaded()))
    {
      this->connectionState = Initializing;

      unsigned int finishTime = UINT_MAX;
      if (SUCCEEDED(result))
      {
        finishTime = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
        if (finishTime != UINT_MAX)
        {
          unsigned int currentTime = GetTickCount();
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentTime, finishTime, finishTime - currentTime);
          this->configuration->Remove(PARAMETER_NAME_FINISH_TIME, true);
        }
      }

      this->mainCurlInstance = new CHttpCurlInstance(&result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // set finish time, all methods must return before finish time
        this->mainCurlInstance->SetFinishTime(finishTime);

        this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
        this->mainCurlInstance->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

        if (this->currentCookies != NULL)
        {
          CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->SetCurrentCookies(this->currentCookies), result, E_HTTP_CANNOT_SET_COOKIES);
        }
      }

      if (SUCCEEDED(result))
      {
        CHttpDownloadRequest *request = new CHttpDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL));
          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
          request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL));
          request->SetUrl(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));
          request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL));
          request->SetStartPosition(this->IsLiveStreamDetected() ? 0 : this->startStreamPosition);
          request->SetEndPosition(this->IsLiveStreamDetected() ? 0 : this->endStreamPosition);

          this->currentStreamPosition = this->startStreamPosition;

          CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->Initialize(request), result, E_HTTP_CANNOT_INITIALIZE);
        }
        FREE_MEM_CLASS(request);
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), this->connectionState = InitializeFailed);

      if (SUCCEEDED(result))
      {
        // all parameters set
        // start receiving data

        CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->StartReceivingData(), result, E_CANNOT_START_RECEIVING_DATA);
      }

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->connectionState = Opening);
    }

    // process stream package (if valid)
    if (streamPackage->GetState() == CStreamPackage::Created)
    {
      HRESULT res = S_OK;
      // stream package is just created, it wasn't processed before
      CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CHECK_POINTER_HRESULT(res, request, res, E_INVALID_STREAM_PACKAGE_REQUEST);

      if (SUCCEEDED(res))
      {
        // set start time of processing request,
        // set Waiting state
        // set response

        CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&res);
        CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

        // allocate memory for response
        CHECK_CONDITION_HRESULT(res, response->GetBuffer()->InitializeBuffer(request->GetLength()), res, E_OUTOFMEMORY);

        if (SUCCEEDED(res))
        {
          streamPackage->GetRequest()->SetStartTime(GetTickCount());
          streamPackage->SetWaiting();
          streamPackage->SetResponse(response);
        }

        CHECK_CONDITION_EXECUTE(FAILED(res), FREE_MEM_CLASS(response));
      }

      CHECK_CONDITION_EXECUTE(FAILED(res), streamPackage->SetCompleted(res));
    }

    if ((streamPackage->GetState() == CStreamPackage::Waiting) || (streamPackage->GetState() == CStreamPackage::WaitingIgnoreTimeout))
    {
      // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
      CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

      // clear response buffer
      response->GetBuffer()->ClearBuffer();

      // first try to find starting media packet (packet which have first data)
      unsigned int packetIndex = UINT_MAX;
      unsigned int foundDataLength = 0;

      int64_t startPosition = request->GetStart();
      packetIndex = this->mediaPackets->GetMediaPacketIndexBetweenPositions(startPosition);

      while (packetIndex != UINT_MAX)
      {
        // get media packet
        CMediaPacket *mediaPacket = this->mediaPackets->GetItem(packetIndex);

        // set copy data start and copy data length
        unsigned int copyDataStart = (startPosition > mediaPacket->GetStart()) ? (unsigned int)(startPosition - mediaPacket->GetStart()) : 0;
        unsigned int copyDataLength = min(mediaPacket->GetLength() - copyDataStart, request->GetLength() - foundDataLength);

        // copy data from media packet to response buffer
        if (this->cacheFile->LoadItems(this->mediaPackets, packetIndex, true, false))
        {
          // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
          response->GetBuffer()->AddToBufferWithResize(mediaPacket->GetBuffer(), copyDataStart, copyDataLength);
        }
        else
        {
          // we can't copy data, try it later
          break;
        }

        // update length of data
        foundDataLength += copyDataLength;

        if (foundDataLength < request->GetLength())
        {
          // find another media packet after end of this media packet
          startPosition += copyDataLength;

          packetIndex = this->mediaPackets->GetMediaPacketIndexBetweenPositions(startPosition);
        }
        else
        {
          // do not find any more media packets for this request because we have enough data
          break;
        }
      }

      if (foundDataLength < request->GetLength())
      {
        // found data length is lower than requested
        // check request flags, maybe we can complete request

        if ((request->IsSetAnyNonZeroDataLength() && (foundDataLength > 0)) ||
          (request->IsSetAnyDataLength()))
        {
          // request can be completed with any length of available data
          streamPackage->SetCompleted(S_OK);
        }
        else
        {
          unsigned int currentTime = GetTickCount();

          if ((streamPackage->GetState() == CStreamPackage::Waiting) && ((currentTime - request->GetStartTime()) > this->GetReceiveDataTimeout()))
          {
            // request timeouted
            // finish request with error to avoid freeze
            this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u' timeout, current time: %u, request start time: %u, specified timeout: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), currentTime, request->GetStartTime(), this->GetReceiveDataTimeout());

            streamPackage->SetCompleted(VFW_E_TIMEOUT);
          }
          else if (this->IsLiveStream() && (!this->IsWholeStreamDownloaded()) && ((currentTime - this->lastReceiveDataTime) > this->GetReceiveDataTimeout()))
          {
            // we don't receive data from protocol at least for specified timeout or request timeouted
            // finish request with error to avoid freeze
            this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u' doesn't receive data for specified time, current time: %u, last received data time: %u, specified timeout: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), currentTime, this->lastReceiveDataTime, this->GetReceiveDataTimeout());

            streamPackage->SetCompleted(VFW_E_TIMEOUT);
          }
          else if ((!this->IsWholeStreamDownloaded()) && (!this->IsStreamLengthEstimated()) && ((request->GetStart() + request->GetLength()) < this->streamLength))
          {
            // we are receiving data, wait for all requested data
          }
          else if (this->IsEndOfStreamReached() && ((request->GetStart() + request->GetLength()) >= this->streamLength))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            streamPackage->SetCompleted((response->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
          }
        }

        if (streamPackage->GetState() == CStreamPackage::Waiting)
        {
          if ((request->GetStart() <= this->currentStreamPosition) && ((request->GetStart() + request->GetLength()) > this->currentStreamPosition))
          {
            // current stream position is within current request
            // we are receiving data, do nothing, just wait for all data
            streamPackage->SetWaitingIgnoreTimeout();
          }
        }

        if (streamPackage->GetState() == CStreamPackage::Waiting)
        {
          if (this->GetSeekingCapabilities() & SEEKING_METHOD_POSITION)
          {
            if (SUCCEEDED(result))
            {
              // find start and end of gap
              // we are missing data without found data length

              int64_t requestStart = request->GetStart() + foundDataLength;
              int64_t requestEnd = requestStart;

              unsigned int startIndex = 0;
              unsigned int endIndex = 0;

              if (this->mediaPackets->GetItemInsertPosition(requestStart, &startIndex, &endIndex))
              {
                // start and end index found successfully

                if (startIndex == endIndex)
                {
                  // this should not happen, because missing data are exactly on start of media packet with startIndex index
                }
                else if ((startIndex == (this->mediaPackets->Count() - 1)) && (endIndex == UINT_MAX))
                {
                  // media packet belongs to end
                  // do nothing, request is from specific point until end of stream
                }
                else if ((startIndex == UINT_MAX) && (endIndex == 0))
                {
                  // this should not happen, because media packet belongs to beginning of media packet collection
                  // data are received from zero position
                }
                else
                {
                  // media packet belongs between packets startIndex and endIndex
                  CMediaPacket *endMediaPacket = this->mediaPackets->GetItem(endIndex);
                  if (endMediaPacket != NULL)
                  {
                    // requests data from requestStart until end packet start position
                    requestEnd = endMediaPacket->GetStart() - 1;
                  }
                }

                if (requestEnd < requestStart)
                {
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u' has start '%lld' after end '%lld', modifying to equal", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), requestStart, requestEnd);
                  requestEnd = requestStart;
                }

                if (this->startStreamPosition != requestStart)
                {
                  // in other case we are trying to open connection with specified stream start position

                  // stops receiving data
                  // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
                  this->StopReceivingData();

                  // re-open connection at requested position
                  this->startStreamPosition = requestStart;
                  this->endStreamPosition = requestEnd;

                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u', requesting data from '%lld' to '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), requestStart, requestEnd);
                }
              }
            }
          }
          else
          {
            // no seeking by position is available
            // check request against current stream position, if we can receive requested data

            if ((request->GetStart() + request->GetLength()) <= this->currentStreamPosition)
            {
              // it's bad, current stream position is after requested data and we can't seek
              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', before current stream position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetStart() + request->GetLength(), this->currentStreamPosition);

              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }
          }
        }
      }
      else if (foundDataLength == request->GetLength())
      {
        // found data length is equal than requested
        streamPackage->SetCompleted(S_OK);
      }
    }

    // store media packets to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();

      if (this->cacheFile->GetCacheFile() == NULL)
      {
        wchar_t *storeFilePath = this->GetStoreFile();
        CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
        FREE_MEM(storeFilePath);
      }

      // in case of live stream remove all downloaded and processed media packets
      // processed media packet means that all data from media packet were requested
      if (this->IsLiveStream() && (this->reportedStreamTime > 0) && (this->reportedStreamPosition > 0) && (this->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_NONE))
      {
        // remove used media packets
        // in case of live stream they will not be needed (after created demuxer and started playing)
        // in case of seeking based on position there can be serious problem, because position in data is not related to play time - switching audio stream will not work

        if (this->mediaPackets->Count() > 0)
        {
          unsigned int mediaPacketRemoveCount = 0;

          while (mediaPacketRemoveCount < this->mediaPackets->Count())
          {
            CMediaPacket *mediaPacket = this->mediaPackets->GetItem(mediaPacketRemoveCount);

            if ((int64_t)this->reportedStreamPosition <= mediaPacket->GetEnd())
            {
              // reported stream position is before media packet end = not whole media packet is processed
              break;
            }

            mediaPacketRemoveCount++;
          }

          if ((mediaPacketRemoveCount > 0) && (this->cacheFile->RemoveItems(this->mediaPackets, 0, mediaPacketRemoveCount)))
          {
            this->mediaPackets->Remove(0, mediaPacketRemoveCount);
          }
        }
      }

      // store all media packets (which are not stored) to file
      if ((this->cacheFile->GetCacheFile() != NULL) && (this->mediaPackets->Count() != 0))
      {
        this->cacheFile->StoreItems(this->mediaPackets, this->lastStoreTime, false);
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    // add current cookies
    if ((this->currentCookies != NULL) && (this->currentCookies->Count() != 0))
    {
      // first add count of cookies
      wchar_t *cookiesCountValue = FormatString(L"%u", this->currentCookies->Count());
      CHECK_POINTER_HRESULT(result, cookiesCountValue, result, E_OUTOFMEMORY); 

      CHECK_CONDITION_HRESULT(result, parameters->Add(PARAMETER_NAME_HTTP_COOKIES_COUNT, cookiesCountValue), result, E_OUTOFMEMORY);

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->currentCookies->Count())); i++)
      {
        CParameter *cookie = this->currentCookies->GetItem(i);
        wchar_t *name = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
        CHECK_POINTER_HRESULT(result, name, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, parameters->Add(name, cookie->GetValue()), result, E_OUTOFMEMORY);

        FREE_MEM(name);
      }

      FREE_MEM(cookiesCountValue);
    }
  }

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configuration);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configuration->Append(parameters);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->StopReceivingData());

  this->connectionState = SUCCEEDED(result) ? Opening : None;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

  this->connectionState = None;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    streamProgress->SetTotalLength(this->streamLength);
    streamProgress->SetCurrentLength(this->currentStreamPosition);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  HRESULT result = __super::ClearSession();
 
  this->streamLength = 0;
  this->receiveDataTimeout = HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->connectionState = None;
  this->cacheFile->Clear();
  this->mediaPackets->Clear();
  this->startStreamPosition = 0;
  this->endStreamPosition = 0;
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;

  FREE_MEM_CLASS(this->currentCookies);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::GetDuration(void)
{
  return this->IsLiveStream() ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetStreamCount(void)
{
  // HTTP protocol has always one stream (container)
  return 1;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetSeekingCapabilities(void)
{
  unsigned int result = SEEKING_METHOD_NONE;

  {
    // lock access to stream
    CLockMutex lock(this->lockMutex, INFINITE);
    
    result = (this->IsWholeStreamDownloaded() || this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED)) ? SEEKING_METHOD_POSITION : SEEKING_METHOD_NONE;
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::SeekToTime(unsigned int streamId, int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = E_SEEK_METHOD_NOT_SUPPORTED;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Http::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::Initialize(CPluginConfiguration *configuration)
{
  CProtocolPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  HRESULT result = ((this->lockMutex != NULL) && (this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->logger->SetParameters(protocolConfiguration->GetConfiguration());
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(protocolConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

    this->receiveDataTimeout = this->configuration->GetValueLong(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT);
    this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED : PROTOCOL_PLUGIN_FLAG_NONE;

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SEEKING_SUPPORT_DETECTION, true, HTTP_SEEKING_SUPPORT_DETECTION_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_SEEKING_SUPPORT_DETECTION : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_http_%s.temp", folder, guid);
    }
    FREE_MEM(guid);
  }

  return result;
}