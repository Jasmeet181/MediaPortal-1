using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.GUI.MSN;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  /// 

  public class GUITVMSNOSD: GUIWindow
  {
    enum Controls 
    {
      Status=2,
			NrOfUsers=3,
      List=50,
			SMSInput=51
    };

    bool m_bNeedRefresh=false;
				
		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion

		bool    m_bPrevOverlay=false;
    DateTime m_dateTime=DateTime.Now;

    public GUITVMSNOSD()
    {
    }

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\TVMSNOSD.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TVMSNOSD;
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    

    public override void Render()
    {
			RenderDlg();
    }

    void HideControl (int dwSenderId, int dwControlID) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID, dwSenderId, dwControlID,0,0,null); 
      OnMessage(msg); 
    }
    void ShowControl (int dwSenderId, int dwControlID) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID, dwSenderId, dwControlID,0,0,null); 
      OnMessage(msg); 
    }

    void FocusControl (int dwSenderId, int dwControlID, int dwParam) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, dwSenderId, dwControlID, dwParam,0,null); 
      OnMessage(msg); 
    }

		#region Base Dialog Members
		public void RenderDlg()
		{
			// render the parent window
			if (null!=m_pParentWindow) 
				m_pParentWindow.Render();
			

			//GUIFontManager.Present();
			// render this dialog box
		
			base.Render();
		}

		void Close()
		{
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
			OnMessage(msg);

			GUIWindowManager.UnRoute();
			m_pParentWindow=null;
			m_bRunning=false;
		}

		public void DoModal(int dwParentId, GUIMessage TriggerMsg)
		{
			m_dwParentWindowID=dwParentId;
			m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
			if (null==m_pParentWindow)
			{
				m_dwParentWindowID=0;
				return;
			}

			GUIWindowManager.RouteToWindow( GetID );

			// active this window...
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
			OnMessage(msg);

			if (TriggerMsg != null)
				OnMessage(TriggerMsg);

			m_bRunning=true;
			while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
			{
				GUIWindowManager.Process();
				System.Threading.Thread.Sleep(100);

        if ((GUIMSNPlugin.CurrentConversation==null) || !GUIMSNPlugin.Messenger.Connected)
        {
          Close();
        }
			}
		}
		#endregion

    public override void OnAction(Action action)
    {
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				Close();
				return;
			}

			if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
			{
				GUISMSInputControl SMSInput= (GUISMSInputControl)GetControl((int)Controls.SMSInput);
				SMSInput.OnAction(action);
				m_bNeedRefresh=true;
				return;
      }
  		    
			if (null!=m_pParentWindow)
			{
				// route unhandled actions to parrent window (otherwise they are left unhandled)
				// actions like Play Pause FF RW 
				m_pParentWindow.OnAction(action);
				return;
			}

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
			string FormattedText="";

      switch ( message.Message )
      {
				case GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED:
				{
					GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED,(int)GUIWindow.Window.WINDOW_MSN_CHAT, 0, 0,0,0,null );
					msg.Label = message.Label;
					msg.SendToTargetWindow = true;
					GUIGraphicsContext.SendMessage(msg);

					string text=String.Format(">{0}", message.Label);
					GUIListItem item =new GUIListItem(text);
					item.IsFolder=false;
					GUIControl.AddListItemControl(GetID,(int)Controls.List,item);
					GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
					list.ScrollToEnd();
					list.Disabled=true;

          SetNrOfPersons();
					m_bNeedRefresh=true;
				}
				break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          base.OnMessage(message);
					GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION, (int)GUIWindow.Window.WINDOW_MSN, GetID, 0,0,0,null );
					msg.SendToTargetWindow = true;
					GUIGraphicsContext.SendMessage(msg);

					GUIGraphicsContext.Overlay=m_bPrevOverlay;
					m_bRunning=false;
					GUIWindowManager.UnRoute();
          FreeResources();
          return true;
        }	    

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
					m_bPrevOverlay=GUIGraphicsContext.Overlay;
					base.OnMessage(message);
					GUIGraphicsContext.Overlay=false;
          GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
          list.WordWrap=true;

          GUIControl.ClearControl(GetID,(int)Controls.List);

          m_bNeedRefresh=false;
          m_dateTime=DateTime.Now;
					SetNrOfPersons();
          return true;
        }

				case GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE:
					FormattedText=String.Format("{0} {1}", message.Label, message.Label3);
					GUIControl.SetControlLabel(GetID,(int)Controls.Status, FormattedText);
					SetNrOfPersons();
					m_bNeedRefresh=true;
					break;

        case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:			
					GUIListItem item2 =new GUIListItem(message.Label);
          item2.IsFolder=false;
          GUIControl.AddListItemControl(GetID,(int)Controls.List,item2);
          GUIListControl list2= (GUIListControl)GetControl((int)Controls.List);
					if (list2!=null)
					{
						list2.ScrollToEnd();
						list2.Disabled=true;
					}
					SetNrOfPersons();
					m_bNeedRefresh=true;
          break;
      }

      return base.OnMessage(message);
    }

		public override void	 ResetAllControls()
    {
      //reset all
      bool bOffScreen=false;
      int iCalibrationY=GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin=0;

      foreach (CPosition pos in m_vecPositions)
      {
        pos.control.SetPosition((int)pos.XPos,(int)pos.YPos+iCalibrationY);
      }
      foreach (CPosition pos in m_vecPositions)
      {
        GUIControl pControl= pos.control;

        int dwPosY=pControl.YPosition;
        if (pControl.IsVisible)
        {
          if ( dwPosY < iTop)
          {
            int iSize=iTop-dwPosY;
            if ( iSize > iMin) iMin=iSize;
            bOffScreen=true;
          }
        }
      }
      if (bOffScreen) 
      {

        foreach (CPosition pos in m_vecPositions)
        {
          GUIControl pControl= pos.control;
          int dwPosX=pControl.XPosition;
          int dwPosY=pControl.YPosition;
          if ( dwPosY < (int)100)
          {
            dwPosY+=Math.Abs(iMin);
            pControl.SetPosition(dwPosX,dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }

    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh) 
      {
        m_bNeedRefresh=false;
        return true;
      }

			GUISMSInputControl SMSInput= (GUISMSInputControl)GetControl((int)Controls.SMSInput);
			if (SMSInput.NeedRefresh())
			{
				return true; 
			}

      return false;
    }
    

		void SetNrOfPersons()
		{

			// Update nr of users
			if (GUIMSNPlugin.CurrentConversation != null)
			{
				string FormattedText=String.Format("{0}: {1}", GUILocalizeStrings.Get(958), GUIMSNPlugin.ContactName);
				GUIControl.SetControlLabel(GetID,(int)Controls.NrOfUsers, FormattedText);
			}
		}
  }
}
  