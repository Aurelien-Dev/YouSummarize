using System;

namespace YouSummarize.Extensions;

public static class Messages
{
    private static readonly string[] ErrorsMessage = [
        "Oops, la recette a brûlé ! 🫠 Réessayez ou rafraîchissez la page !",
        "Zut, on a mis trop de sel ! 😅 Une erreur s'est glissée dans la recette, essayez à nouveau !",
        "Oops, le soufflé est retombé... 😢 Réessayez ou rafraîchissez la page !",
        "Oh non, la pâte n’a pas levé ! 😵‍💫 Essayez encore ou rafraîchissez la page.",
        "Aïe, on a renversé la casserole ! 🍲💥 Réessayez, ça devrait mieux marcher.",
        "On a oublié le timer et tout a cramé... 🔥 Relançons la recette !",
        "Oups, un ingrédient manque ! 🧑‍🍳 Essayez encore ou rafraîchissez la page.",
        "Mince, la sauce a tourné... 😬 On refait un essai ?",
        "Ah zut, le robot mixeur a explosé ! 🤖💨 Retentez votre chance.",
        "Ding ! Mais... où est le plat ? 😳 Une erreur est survenue, essayez à nouveau.",
        "On a suivi la recette, mais ça a fini en omelette... 🥚 Essayez encore !",
        "Oops, notre cuistot a mis trop de piment ! 🌶️ Rafraîchissez la page pour une nouvelle tentative.",
        "Catastrophe en cuisine ! 😱 Retentons la préparation ensemble."
    ];

    private static readonly string[] LatestRecipeTitles = [
        "Tout juste sortis du four 🔥",
        "Recettes toutes chaudes 😋",
        "Fraîchement mijoté 🫕",
        "Les dernières inspirations du chef 👨‍🍳",
        "Tout droit sorties des casseroles 🥘",
        "Encore fumantes ! 💨",
        "Nouvelles créations maison 🏠",
        "Ça sent bon la nouveauté ! 👃",
        "Recettes chaudes bouillantes ! 🍛",
        "Fraîchement sorties du carnet de recettes 📓",
        "Les petits derniers du fourneau 🧁",
        "À déguster sans attendre ! 🍽️",
        "Les plats du moment 🌟",
        "Tout juste dressés dans l’assiette 🍽️",
        "Les dernières inspirations du chef 🍽️"
    ];

    public static string GetRandomLatestRecipeTitles()
    {
        return GetRandomString(LatestRecipeTitles);
    }
    public static string GetRandomErrorMessage()
    {
        return GetRandomString(ErrorsMessage);
    }

    private static string GetRandomString(string[] strings)
    {
        var random = new Random();
        var index = random.Next(strings.Length);
        return strings[index];
    }
}
