namespace VoiceBot;

public interface IPieceManager
{
    Task<(Stream, string)> WriteFile(List<VoicePiece> pieceList);
    UserStream MergePCM(List<UserStream> userStreams);
}