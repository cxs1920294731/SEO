Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Configuration
Imports EDM4Everbuying

Public Class Service1


    Private listLog As New List(Of AutoSentLog)
    Protected Overrides Sub OnStart(ByVal args() As String)
        ' 请在此处添加代码以启动您的服务。此方法应完成设置工作，
        ' 以使您的服务开始工作。
        EventLog.WriteEntry("Start service")
        Common.LogText("Service Start")
        'CheckSchedule()
        '  MainTimer.Interval = ConfigurationManager.AppSettings("Interval")
    End Sub

    Protected Overrides Sub OnStop()
        ' 在此处添加代码以执行任何必要的拆解操作，从而停止您的服务。
    End Sub



    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>


    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    Private Sub MainTimer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles MainTimer.Elapsed
        Me.MainTimer.Enabled = False
        CheckSchedule()
        Me.MainTimer.Enabled = True
    End Sub


    ''' <summary>
    ''' Check if any new email campaigns to send 
    ''' </summary>
    ''' <remarks></remarks>
    Sub CheckSchedule()

        'Common.LogText("Schedule Start")
        'System.Threading.Thread.Sleep(1000)


        Dim A As New alerter
        Try
            Dim autoTest As New AutoModel()

            autoTest.GetRssSubscriptionAndInsertAndSentEmail(listLog)


            Dim hour As Integer = ConfigurationManager.AppSettings("hourforSitemap").ToString().Trim()
            Dim minute As Integer = ConfigurationManager.AppSettings("minuteforSitemap").ToString().Trim()

            'If (DateTime.Now.Hour = hour And DateTime.Now.Minute > minute And DateTime.Now.Minute <= minute + 2) Then
            If 1 Then
                Common.LogText("sitemap start")
                GetSiteMap.SiteMap()
                Common.LogText("sitemap end")
            End If

        Catch ex As Exception
            Common.LogException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' 
    ''' 
    ''' </summary>
    ''' <param name="r"></param>
    ''' <remarks></remarks>
    Sub SendEmail(ByVal r As RssSubscription)
        Dim A As New alerter
        Try
            '龙震天博客
            If r.rssType.Trim() = "B" Then
                Try
                    Dim B As New Blog
                    Dim EmailCampaignElements = B.ReadRSS(r.Url)
                    Dim CustomizedTemplate As String = B.CustomizeTempalate(r.Template, EmailCampaignElements)
                    Dim Subject As String = B.CreateCampaignBlog(r.SpreadLogin, r.AppID, r.SenderName, r.SenderEmail, CustomizedTemplate, EmailCampaignElements)
                    A.UpdateSentLog(r.RssID, Subject)
                    'A.LogText(Subject)
                    NotificationEmail.SentStartEmail(Subject, CustomizedTemplate)
                Catch ex As Exception
                    'A.LogText(ex.ToString)
                    NotificationEmail.SentErrorEmail(ex.ToString())
                End Try

                EventLog.WriteEntry("succeed to send email")

            ElseIf r.rssType.Trim() = "M" Then
                Dim M As New Ladies
                M.SendEmailMagazine(r)

                'GroupBuyer项目入口
            ElseIf r.rssType.Trim() = "D" Then
                Dim D As New GroupBuyer2
                D.SendEmailDeal(r)

                'E stands for Everbuying
            ElseIf r.rssType.Trim().StartsWith("E") Then
                Dim E As New Everbuying
                Select Case r.rssType.Trim()
                    Case "EA" : E.SendEmailForAll(r)
                    Case "EP" : E.SendPersonalDefaultPageByCategory(r)
                    Case "EC" : E.SendEmailByCategories(r)
                    Case "ED" : E.SendEmailByNoCategory(r)
                    Case "EN" : E.SendNotOpen(r)
                End Select
            End If

        Catch ex As Exception
            Common.LogText(ex.ToString)
        End Try
    End Sub
End Class
