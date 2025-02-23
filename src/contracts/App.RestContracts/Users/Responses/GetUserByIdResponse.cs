using App.RestContracts.Users.Models;

namespace App.RestContracts.Users.Responses;

public record GetUserByIdResponse(UserModel User);