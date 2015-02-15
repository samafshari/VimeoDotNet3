This is the VimeoDotNet wrapper for Vimeo API 3.0. It allows you to authorize and get access tokens and send requests to Vimeo.

For a detailed user's guide, and how to upload, please visit http://redcorners.com.

#First time authorization:

string url = Vimeo.VimeoClient.GetAuthURL(cid: **YOUR_API_KEY**, redirect: **REDIRECT_URL**);

//[open the url and get the access code from the redirect url]

var vc = Vimeo.VimeoClient.Authorize(
 authCode: VERIFIER,
 cid: YOUR_API_KEY,
 secret: YOUR_API_SECRET,
 redirect: REDIRECT_URL);
 
Console.WriteLine(vc.AccessToken);

#Authorization with access token:
var vc = Vimeo.VimeoClient.ReAuthorize(accessToken, APIKey, APISecret);

#Calling a method:
Dictionary<string, object> me = vc.Request("/me", null, "GET");

Console.WriteLine(me["account"].ToString())

#Uploading a file:
string video_id = vc.Upload(“D:/Video.mp4”);

#Uploading with resume support:
var ticket = vc.GetTicket();

Settings.Default.CompleteUri = ticket.CompleteUri;

Settings.Default.TicketId = ticket.TicketId;

Settings.Default.UploadLink = ticket.UploadLinkSecure;

Settings.Default.Save();

string video_id = vc.Upload(“D:/Video.mp4”, ticket);

#Replacing an existing video:
var ticket = vc.GetTicket(videoId);

string video_id = vc.Upload(“D:/Video.mp4”, ticket);

#Upload Progress Notification:
vc.UploadCallback = UploadCallback;

string video_id = vc.Upload(“D:/Video.mp4”);

void UploadCallback(VerifyFeedback feedback)

{

Console.WriteLine("{0}/{1} bytes uploaded.", feedback.LastByte, feedback.ContentSize);

}
