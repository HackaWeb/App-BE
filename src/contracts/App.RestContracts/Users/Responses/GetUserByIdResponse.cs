using App.RestContracts.Models;

namespace App.RestContracts.Users.Responses;

public record GetUserByIdResponse(UserModel User);