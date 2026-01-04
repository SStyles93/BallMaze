// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("4sCgw1gSDn65wqxw701E6yzT0ls+KffqAveu786FO3FrXSQnouTJsvvQd6h8mtBqD2ij37lnbbSkpG0o5VfU9+XY09z/U51TItjU1NTQ1dbeztqnCDG57B2wfZEfmTVCN1py3lKaq3RvLoPkpAgg5CekSZYUItdQnTchmH3x5KSlU/I1LMA3mp3SJZV2igu9a43m8PB9uGmFxyUUBmvt9GNGpfwl1ewOU6tAeOgAw4vvTd060XwNqvPOrQ7eOXbjFLXHuJqK2oZX1NrV5VfU39dX1NTVfvOL+KH+T0XaT6prpG879poajsbgqs17VoBD93z/ztwF5U3Ym+Jf6bI10nYy0PgnasaDXbHbHsZsXJsVwNK9HGZd8FSknFFl/yIsDtfW1NXU");
        private static int[] order = new int[] { 8,10,12,8,11,7,8,8,12,13,10,11,12,13,14 };
        private static int key = 213;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
