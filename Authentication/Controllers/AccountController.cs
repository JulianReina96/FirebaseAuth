using Firebase.Auth;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

using Newtonsoft.Json;
using FirebaseAdmin;

using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Authentication.Models;
using Authentication.JsonFire.ModelJsonFire;

namespace Authentication.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AccountController : Controller
	{


		FirebaseAuthProvider auth;

		public AccountController()
		{
			auth = new FirebaseAuthProvider(
							new FirebaseConfig("AIzaSyA9SvJlPbX6b9TKRrMsOzRT1yTiCwA4t1Y"));

		}


		[Authorize/*(Policy = "AdminOnly")*/]
		[HttpPost("Register")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Register(AdministradorDTO user)
		{

			try
			{//create the user
				ModelJsonFire url = new ModelJsonFire();


				await auth.CreateUserWithEmailAndPasswordAsync(user.Email, user.Senha);
				var newUser = await auth.SignInWithEmailAndPasswordAsync(user.Email, user.Senha);

				// criação de role ADMIN para novos usuarios cadastrados pelo administrador
				var token = newUser.FirebaseToken;

				var claims = new Dictionary<string, object>()
				{
					{ "admin", true },
				};
				if (FirebaseApp.DefaultInstance == null)

				{
					FirebaseApp.Create(new AppOptions()
					{
						Credential = GoogleCredential.FromFile(url.UrlJsonAPI),
					});
				}
				var decoded = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
				var uid = decoded.Uid;
				await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);

				return Ok();
			}
			catch (FirebaseAuthException ex)
			{
				if (ex.Reason.ToString() == "EmailExists")
				{
					TempData["ErrorMessage"] = "Email já pertence a um administrador";
					return NotFound();
				}
				else
				{
					return BadRequest();
				}
			}
		}


		[HttpPost("Signin")]
		public async Task<IResult> SignIn(AdministradorDTO user)
		{

			FirebaseAuthLink firebaseAuthLink = null;


			//log in the user
			firebaseAuthLink = await auth
							.SignInWithEmailAndPasswordAsync(user.Email, user.Senha);
			if (firebaseAuthLink.FirebaseToken != null)
			{
				HttpContext.Session.SetString("_UserToken", firebaseAuthLink.FirebaseToken);
				var usertoken = HttpContext.Session.GetString("_UserToken");
				HttpClient httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {firebaseAuthLink.FirebaseToken}");


				return Results.Accepted(usertoken);
			}
			else
			{

				return Results.BadRequest();
			}




		}

		//saving the token in a session variable

		[HttpPost("Logout")]
		public async Task<IResult> Logout()
		{
			string token = "";

			HttpContext.Session.SetString("_UserToken", token);
			return Results.Accepted();

		}

	}
}




