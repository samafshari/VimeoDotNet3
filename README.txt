This is the VimeoDotNet wrapper for Vimeo API 3.0. 
It allows you to authorize and get access tokens and
send requests to Vimeo.

Here's a quick start guide:

Step 1.
	Let's assume you have these variables set with what you have
	on your application's page on Vimeo Developers Portal

public static string APIKey;
public static string APISecret;
public static string RedirectURL;

Step 2. (First time authorization:)
	Make the user open the url generated using the 
	Vimeo.VimeoClient.GetAuthURL method:

Vimeo.VimeoClient.GetAuthURL(APIKey, new List<string>() { "public" }, RedirectURL));

Step 3.
	After the user opens this URL and authorizes your app to do stuff,
	Vimeo will redirect them to your specified RedirectURL. It will look
	like http://localhost:50000/?code=SOMELONGCODE where 
	RedirectURL=http://localhost:50000 and code=SOMELONGCODE is the response
	that contains your authCode. So SOMELONGCODE is the authCode we will use
	in the future.

	Make a VimeoClient object which enables you to communicate with Vimeo:

var vc = Vimeo.VimeoClient.Authorize(authCode, APIKey, APISecret, RedirectURL);

	Now save vc.AccessToken somewhere, so next time you can use it to quickly
	reauthorize.

Step 4. Call a method (https://developer.vimeo.com/api/endpoints)
	For example we want to GET "/me":

Dictionary<string, object> me = vc.Request("/me", null, "GET");

	Now "me" contains a deserialized JSON that looks like this:
	{u'account': u'basic',
 u'bio': u'Twitter: twitter.com/saeedafshariFacebook: facebook.com/saeedafshariDownload Vimeo for Windows 2.0:http://tinyurl.com/vimeoforwindowsUse Vimeo API in .NET: http://afsharious.wordpress.com/vimeodotnet',
 u'content_filter': [u'language',
                     u'drugs',
                     u'violence',
                     u'nudity',
                     u'safe',
                     u'unrated'],
 u'created_time': u'2008-08-15T10:09:40+00:00',
 u'link': u'https://vimeo.com/saeed',
 u'location': None,...

	For example to access the value of "account", you can do:

Console.WriteLine(me["account"].ToString())

	and the response should be "basic" if you have a basic account.

Happy Vimeoing!


PS. (Re-authorization:)
	If you already have an AccessToken, you can perform this step to reauthorize,
	and you don't need to perform Steps 1, 2 and 3.

var vc = Vimeo.VimeoClient.ReAuthorize(accessToken, APIKey, APISecret);

	