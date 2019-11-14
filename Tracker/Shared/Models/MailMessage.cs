using DSM.Core.Ops;
using System.IO;

namespace DSM.Controller.Tracker.Shared.Models
{
    public class MailMessage
    {
        private string _mailContent = string.Empty;

        public MailMessage()
        {
            string filePath = FileOperations.AssemblyDirectory + @"\MailContent.html";
            StreamReader contentReader = new StreamReader(filePath);

            _mailContent = contentReader.ReadToEnd();
        }

        private string _mailTitle = string.Empty;
        private string _mailSubTitle = string.Empty;
        private string _mailStatus1 = string.Empty;
        private string _mailStatus2 = string.Empty;
        private string _mailMachineName = string.Empty;
        private string _mailSiteUrl = string.Empty;
        private string _mailSiteName = string.Empty;
        private string _mailSiteAvailable = string.Empty;
        private string _mailCheckTime = string.Empty;
        private string _mailAppVersion = string.Empty;
        private string _mailLeftImage = string.Empty;
        private string _mailTitleColor = string.Empty;

        public string MailContent => _mailContent;
        public string MailTitle
        {
            get => _mailTitle;
            set
            {
                _mailTitle = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_TITLE, value);
            }
        }
        public string MailSubTitle
        {
            get => _mailSubTitle;
            set
            {
                _mailSubTitle = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_SUBTITLE, value);

            }
        }
        public string MailStatus1
        {
            get => _mailStatus1;
            set
            {
                _mailStatus1 = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_STATUS1, value);
            }
        }
        public string MailStatus2
        {
            get => _mailStatus2;
            set
            {
                _mailStatus2 = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_STATUS2, value);
            }
        }
        public string MailMachineName
        {
            get => _mailMachineName;
            set
            {
                _mailMachineName = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_MACHINE_NAME, value);
            }
        }
        public string MailSiteUrl
        {
            get => _mailSiteUrl;
            set
            {
                _mailSiteUrl = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_SITE_URL, value);
            }
        }
        public string MailSiteName
        {
            get => _mailSiteName;
            set
            {
                _mailSiteName = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_SITE_NAME, value);
            }
        }
        public string MailSiteAvailable
        {
            get => _mailSiteAvailable;
            set
            {
                _mailSiteAvailable = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_SITE_AVAILABLE, value);
            }
        }
        public string MailCheckTime
        {
            get => _mailCheckTime;
            set
            {
                _mailCheckTime = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_CHECK_TIME, value);
            }
        }
        public string MailAppVersion
        {
            get => _mailAppVersion;
            set
            {
                _mailAppVersion = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_APP_VERSION, value);
            }
        }
        public string MailLeftImage
        {
            get => _mailLeftImage;
            set
            {
                _mailLeftImage = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_LEFT_IMAGE, value);
            }
        }
        public string MailTitleColor
        {
            get => _mailTitleColor;
            set
            {
                _mailTitleColor = value;
                _mailContent = _mailContent.Replace(MailMethod.MAIL_TITLE_COLOR, value);
            }
        }
    }
}
