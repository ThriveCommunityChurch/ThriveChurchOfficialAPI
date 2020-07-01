using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class SetConfigRequest
    {
        /// <summary>
        /// A collection of configurations to set
        /// </summary>
        public IEnumerable<ConfigurationMap> Configurations { get; set; }

        /// <summary>
        /// Validate the request object
        /// </summary>
        /// <returns></returns>
        public static ValidationResponse Validate(SetConfigRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            if (request.Configurations == null || !request.Configurations.Any())
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Configurations)));
            }

            foreach (var config in request.Configurations)
            {
                switch (config.Type)
                {
                    case ConfigType.Email:

                        var emailValidation = ValidateEmail(config.Value);
                        if (emailValidation.HasErrors)
                        {
                            return new ValidationResponse(true, emailValidation.ErrorMessage);
                        }

                        break;
                    case ConfigType.Link:

                        var uriValidation = ValidateUri(config.Value);
                        if (uriValidation.HasErrors)
                        {
                            return new ValidationResponse(true, uriValidation.ErrorMessage);
                        }

                        break;

                    case ConfigType.Phone:

                        var phoneValidation = ValidatePhone(config.Value);
                        if (phoneValidation.HasErrors)
                        {
                            return new ValidationResponse(true, phoneValidation.ErrorMessage);
                        }

                        break;
                    case ConfigType.Social:

                        var socialValidation = ValidateSocial(config);
                        if (socialValidation.HasErrors)
                        {
                            return new ValidationResponse(true, socialValidation.ErrorMessage);
                        }

                        break;
                }
            }

            return new ValidationResponse("Success!");
        }

        /// <summary>
        /// Validate an email configuration
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private static ValidationResponse ValidateEmail(string email)
        {
            var address = new EmailAddressAttribute();

            if (!address.IsValid(email))
            {
                return new ValidationResponse(true, string.Format(SystemMessages.ConfigNotProperlyFormatted, "Email", email));
            }

            return new ValidationResponse("Success!");  
        }

        /// <summary>
        /// Validate an email configuration
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        private static ValidationResponse ValidatePhone(string phone)
        {
            if (phone.Contains(' ') || phone.Contains('(') || phone.Contains(')') || phone.Contains('-') || phone.Contains('+'))
            {
                return new ValidationResponse(true, SystemMessages.PhoneNumbersCannotContainSpecialCharactersOrSpaces);
            }

            var validInt = !Int32.TryParse(phone, out Int32 _);

            if (!validInt)
            {
                return new ValidationResponse(true, SystemMessages.PhoneNumbersCannotContainSpecialCharactersOrSpaces);
            }

            return new ValidationResponse("Success!");
        }
        
        /// <summary>
        /// Validate an email configuration
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        private static ValidationResponse ValidateSocial(ConfigurationMap config)
        {
            if (config.Value.Contains(' '))
            {
                return new ValidationResponse(true, SystemMessages.SocialConfigsCannotContainSpaces);
            }

            if (config.Key == "FB_PageId")
            {
                var validInt = !long.TryParse(config.Value, out long _);

                if (!validInt)
                {
                    return new ValidationResponse(true, SystemMessages.InvalidConfigForFBPage);
                }
            }
            else if (config.Key == "IG_uName" || config.Key == "TW_uName")
            {
                if (config.Value.Contains("?") || config.Value.Contains("&") || config.Value.Contains("="))
                {
                    return new ValidationResponse(true, SystemMessages.SocialConfigsContainInvalidCharacters);
                }

                // if anyone tries to put @ in their username we need to remove that
                config.Value = config.Value.Replace("@", "").Trim();
            }

            return new ValidationResponse("Success!");
        }

        /// <summary>
        /// Validate a uri configuration
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static ValidationResponse ValidateUri(string uri)
        {
            var validUri = Uri.IsWellFormedUriString(uri, UriKind.Absolute);

            if (!validUri)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.ConfigNotProperlyFormatted, "Link", uri));
            }

            return new ValidationResponse("Success!");
        }
    }
}