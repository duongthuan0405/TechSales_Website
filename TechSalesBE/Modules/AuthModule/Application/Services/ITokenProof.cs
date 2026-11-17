namespace AuthModule.Application.Services;
internal interface ITokenProof
{
    string Hash(string value);
    bool Matches(string value, string hash);
}
