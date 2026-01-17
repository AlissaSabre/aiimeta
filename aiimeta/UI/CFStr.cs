using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.UI
{
    /// <summary>List of CFSTR_* constants as defined in Windows SDK.</summary>
    public static class CFStr
    {
        public const string ACLUI_SID_INFO_LIST = "CFSTR_ACLUI_SID_INFO_LIST";
        public const string AUTOPLAY_SHELLIDLISTS = "Autoplay Enumerated IDList Array";
        public const string DROPDESCRIPTION = "DropDescription";
        public const string DSDISPLAYSPECOPTIONS = "DsDisplaySpecOptions";
        public const string DSOBJECTNAMES = "DsObjectNames";
        public const string DSOP_DS_SELECTION_LIST = "CFSTR_DSOP_DS_SELECTION_LIST";
        public const string DSPROPERTYPAGEINFO = "DsPropPageInfo";
        public const string DSQUERYPARAMS = "DsQueryParameters";
        public const string DSQUERYSCOPE = "DsQueryScope";
        public const string DS_DISPLAY_SPEC_OPTIONS = "DsDisplaySpecOptions";
        public const string ENTERPRISE_ID = "EnterpriseDataProtectionId";
        public const string FILECONTENTS = "FileContents";
        public const string FILEDESCRIPTORA = "FileGroupDescriptor";
        public const string FILEDESCRIPTORW = "FileGroupDescriptorW";
        public const string FILENAMEA = "FileName";
        public const string FILENAMEW = "FileNameW";
        public const string FILENAMEMAPA = "FileNameMap";
        public const string FILENAMEMAPW = "FileNameMapW";
        public const string FILE_ATTRIBUTES_ARRAY = "File Attributes Array";
        public const string HYPERLINK = "Hyperlink";
        public const string INDRAGLOOP = "InShellDragLoop";
        public const string INETURLA = "UniformResourceLocator";
        public const string INETURLW = "UniformResourceLocatorW";
        public const string INVOKECOMMAND_DROPPARAM = "InvokeCommand DropParam";
        public const string LOGICALPERFORMEDDROPEFFECT = "Logical Performed DropEffect";
        public const string MIME_AIFF = "audio/aiff";
        public const string MIME_APPLICATION_JAVASCRIPT = "application/javascript";
        public const string MIME_APP_XML = "application/xml";
        public const string MIME_AVI = "video/avi";
        public const string MIME_BASICAUDIO = "audio/basic";
        public const string MIME_BMP = "image/bmp";
        public const string MIME_DDS = "image/vnd.ms-dds";
        public const string MIME_FRACTALS = "application/fractals";
        public const string MIME_GIF = "image/gif";
        public const string MIME_HTA = "application/hta";
        public const string MIME_HTML = "text/html";
        public const string MIME_JPEG = "image/jpeg";
        public const string MIME_JPEG_XR = "image/vnd.ms-photo";
        public const string MIME_MANIFEST = "text/cache-manifest";
        public const string MIME_MPEG = "video/mpeg";
        public const string MIME_PDF = "application/pdf";
        public const string MIME_PJPEG = "image/pjpeg";
        public const string MIME_PNG = "image/png";
        public const string MIME_POSTSCRIPT = "application/postscript";
        public const string MIME_QUICKTIME = "video/quicktime";
        public const string MIME_RAWDATA = "application/octet-stream";
        public const string MIME_RAWDATASTRM = "application/octet-stream";
        public const string MIME_RICHTEXT = "text/richtext";
        public const string MIME_SVG_XML = "image/svg+xml";
        public const string MIME_TEXT = "text/plain";
        public const string MIME_TEXT_JSON = "text/json";
        public const string MIME_TIFF = "image/tiff";
        public const string MIME_TTAF = "application/ttaf+xml";
        public const string MIME_TTML = "application/ttml+xml";
        public const string MIME_WAV = "audio/wav";
        public const string MIME_WEBVTT = "text/vtt";
        public const string MIME_XBM = "image/xbm";
        public const string MIME_XHTML = "application/xhtml+xml";
        public const string MIME_XML = "text/xml";
        public const string MIME_X_AIFF = "audio/x-aiff";
        public const string MIME_X_BITMAP = "image/x-xbitmap";
        public const string MIME_X_EMF = "image/x-emf";
        public const string MIME_X_ICON = "image/x-icon";
        public const string MIME_X_JAVASCRIPT = "application/x-javascript";
        public const string MIME_X_MIXED_REPLACE = "multipart/x-mixed-replace";
        public const string MIME_X_MSVIDEO = "video/x-msvideo";
        public const string MIME_X_PNG = "image/x-png";
        public const string MIME_X_REALAUDIO = "audio/x-pn-realaudio";
        public const string MIME_X_SGI_MOVIE = "video/x-sgi-movie";
        public const string MIME_X_WAV = "audio/x-wav";
        public const string MIME_X_WMF = "image/x-wmf";
        public const string MOUNTEDVOLUME = "MountedVolume";
        public const string NETRESOURCES = "Net Resource";
        public const string PASTESUCCEEDED = "Paste Succeeded";
        public const string PERFORMEDDROPEFFECT = "Performed DropEffect";
        public const string PERSISTEDDATAOBJECT = "PersistedDataObject";
        public const string PREFERREDDROPEFFECT = "Preferred DropEffect";
        public const string PRINTERGROUP = "PrinterFriendlyName";
        public const string SHELLDROPHANDLER = "DropHandlerCLSID";
        public const string SHELLIDLIST = "Shell IDList Array";
        public const string SHELLIDLISTOFFSET = "Shell Object Offsets";
        public const string SHELLURL = "UniformResourceLocator";
        public const string TARGETCLSID = "TargetCLSID";
        public const string UNTRUSTEDDRAGDROP = "UntrustedDragDrop";
        public const string VFW_FILTERLIST = "Video for Windows 4 Filters";
        public const string WIAITEMNAMES = "WIAItemNames";
        public const string WIAITEMPTR = "WIAItemPointer";
        public const string ZONEIDENTIFIER = "ZoneIdentifier";

        // _Unicode_ aliases
        public const string FILEDESCRIPTOR = FILEDESCRIPTORW;
        public const string FILENAME = FILENAMEW;
        public const string FILENAMEMAP = FILENAMEMAPW;
        public const string INETURL = INETURLW;

        // I don't know when and how we could use this.
        public const string? MIME_NULL = null;
    }
}
