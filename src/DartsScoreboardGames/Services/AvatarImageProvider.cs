namespace DartsScoreboardGames.Services {
    public class AvatarImageProvider {

        private Lazy<IEnumerable<string>> _avatarImagePaths;

        public AvatarImageProvider() {
            _avatarImagePaths = new(EnumerateImagePaths);
        }


        private IEnumerable<string> EnumerateImagePaths() {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            string[] images = Directory.GetFiles(folderPath, "*");

            return images.Select(x => x.Replace(folderPath, "avatars"));
        }

        public IEnumerable<string> Avatars =>
            _avatarImagePaths.Value;

    }
}
