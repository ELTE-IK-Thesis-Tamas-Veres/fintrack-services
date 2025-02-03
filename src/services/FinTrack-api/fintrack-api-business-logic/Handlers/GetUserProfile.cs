using fintrack_common.Configuration;
using fintrack_common.DTO.AuthDTO;
using fintrack_common.Providers;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers
{
    public class GetUserProfileCommand : IRequest<LoginResponse>
    {
        public string IdToken { get; set; } = "";
    }

    public class GetUserProfileCommandHandler : IRequestHandler<GetUserProfileCommand, LoginResponse>
    {
        private AuthenticationConfiguration _options;
        private readonly IDateTimeProvider _dateProvider;
        public GetUserProfileCommandHandler(IOptions<AuthenticationConfiguration> options, IDateTimeProvider dateTimeProvider)
        {
            _options = options.Value;
            _dateProvider = dateTimeProvider;
        }
        public async Task<LoginResponse> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
        {
            string? userEmail = GetValueFromTokenByName(request.IdToken, "preferred_username");

            if (String.IsNullOrEmpty(userEmail))
            {
                throw new UnauthorizedAccessException("Can't get 'preferred_username' from id token");
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim("userId", "gggg"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, _dateProvider.GetUTCNowWithOffset().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer));

            // claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            var tokenOptions = new JwtSecurityToken(
                 claims: claims,
                 expires: DateTime.Now.AddDays(_options.Lifetime),
                 signingCredentials: signingCredentials
             );

            var tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenOptions);

            if (String.IsNullOrEmpty(token))
            {
                throw new Exception("Access token generation failed");
            }

            return new LoginResponse()
            {
                AccessToken = token
            };
        }

        private string? GetValueFromTokenByName(string token, string name)
        {
            if (string.IsNullOrEmpty(token) == false)
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jsonToken = new JwtSecurityToken();
                try
                {
                    var tokenRead = handler.ReadToken(token);
                    if (tokenRead is JwtSecurityToken jwtToken)
                    {
                        jsonToken = jwtToken;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }

                return jsonToken.Claims.FirstOrDefault(claim => claim.Type == name)?.Value;
            }

            return null;
        }
    }
}
