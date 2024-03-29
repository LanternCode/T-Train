﻿using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace ClassLibrary
{
    public abstract class clsAbstractUser
    {
        protected Int32 mID;
        protected string mEmail;
        protected string mFirstName;
        protected string mLastName;
        public int ID
        {
            get { return mID; }
            set { mID = value; }
        }
        public string Email
        {
            get { return mEmail; }
            set { mEmail = value; }
        }
        public string FirstName
        {
            get { return mFirstName; }
            set { mFirstName = value; }
        }
        public string LastName
        {
            get { return mLastName; }
            set { mLastName = value; }
        }

        public string GetHashPassword(string ToHash)
        {
            //Generates a SHA512 hash of a given password
            //SHA512 is used here because it is the best hashing algorithm the System.Security.Cryptography provides
            //In real world use, a better hashing algorithm would be used, like SHA3

            if (ToHash != "")
            {
                SHA512Managed HashGen = new SHA512Managed();
                string HashString;
                byte[] TextBytes;
                byte[] HashBytes;

                TextBytes = System.Text.Encoding.UTF8.GetBytes(ToHash);
                HashBytes = HashGen.ComputeHash(TextBytes);
                HashString = BitConverter.ToString(HashBytes).Replace("-", "");
                return HashString;
            }
            else return "";
        }

        public string SendResetEmail(string System)
        {
            //This function checks which system it is being called for, and changes the specified user's password
            //To a randomised code. This code is emailed to the user, and can be provided as a password change
            //authentication method.
            string Sproc = "sproc_tblCustomer_ForgotPassword";
            string TempPW = "";

            //Generate hash based on a random number
            Random random = new Random();
            TempPW = GetHashPassword(random.Next(1, 9999999).ToString()).Substring(0, 50);

            clsDataConnection DB = new clsDataConnection();
            DB.AddParameter("@Email", mEmail);
            DB.AddParameter("@Password", TempPW);
            DB.Execute(Sproc);

            //Set up to send EMails from a GMail account
            string Email = "teamdroptable2020@gmail.com";
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(Email, "Teamdroptable123!"),
                EnableSsl = true,
            };

            //Can't do redirects to pages with query strings because it's all localhost and runs on different ports
            //Uses hash as a code for validation
            smtpClient.Send(Email, mEmail, $"Change password for {System} System", $"Your code is: {TempPW}");

            return TempPW;
        }
    }
}
