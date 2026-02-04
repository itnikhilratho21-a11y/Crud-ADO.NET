using Crud.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Crud.Controllers
{
    public class UserController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;

        //  REGISTER (GET) 
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // REGISTER (POST) 
        [HttpPost]
        public ActionResult Register(User u)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                //  Check if email already exists
                SqlCommand checkCmd =
                    new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@e", con);
                checkCmd.Parameters.AddWithValue("@e", u.Email);

                con.Open();
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    ViewBag.Error = "Email already registered. Please login.";
                    return View();
                }

                //  Insert new user
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Users
                    (Username, Email, Password, CityName, StateName)
                    VALUES
                    (@u, @e, @p, @c, @s)", con);

                cmd.Parameters.AddWithValue("@u", u.Username);
                cmd.Parameters.AddWithValue("@e", u.Email);
                cmd.Parameters.AddWithValue("@p", u.Password);
                cmd.Parameters.AddWithValue("@c", u.CityName);
                cmd.Parameters.AddWithValue("@s", u.StateName);

                cmd.ExecuteNonQuery();
            }

            // Auto-login after successful registration
            Session["user"] = u.Email;

            //  Redirect to Dashboard
            return RedirectToAction("Dashboard");
        }

        //  LOGIN (GET) 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // LOGIN (POST) 
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Email=@e AND Password=@p", con);

                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);

                con.Open();
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    Session["user"] = email;
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.Error = "Invalid Email or Password";
                    return View();
                }
            }
        }

        //  DASHBOARD 
        public ActionResult Dashboard()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login");

            ViewBag.Email = Session["user"].ToString();
            return View();
        }

        //  LOGOUT 
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}