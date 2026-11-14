using Sep2026.ClinicPatientRecord.API.Models;
using Sep2026.ClinicPatientRecord.API.Repositories;

namespace Sep2026.ClinicPatientRecord.API.Service
{
    public class TokenService
    {
        private readonly TokenRepository _tokenRepository;

        public TokenService(TokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public async Task<Token> GenerateAsync(
            int patientId, int doctorId)
        {
            return await _tokenRepository.GenerateAsync(
                patientId, doctorId);
        }
    }
}