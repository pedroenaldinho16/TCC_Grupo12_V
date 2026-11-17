namespace TCC_Grupo12_III.Models
{
    /// <summary>
    /// Recorte aproximado da Cantareira já adotado pelo projeto para seus registros.
    /// Não representa um limite oficial do relevo nem de uma unidade de conservação.
    /// Cada vértice é um par [latitude, longitude].
    /// </summary>
    public static class LimitesSerraCantareira
    {
        public static readonly double[][] Contorno =
        {
            new[] { -23.30967, -46.50179 },
            new[] { -23.30967, -46.48505 },
            new[] { -23.30967, -46.46787 },
            new[] { -23.31299, -46.45068 },
            new[] { -23.31798, -46.43349 },
            new[] { -23.32753, -46.41721 },
            new[] { -23.33583, -46.40002 },
            new[] { -23.35037, -46.39776 },
            new[] { -23.36615, -46.40410 },
            new[] { -23.37694, -46.41992 },
            new[] { -23.38524, -46.43711 },
            new[] { -23.39977, -46.44842 },
            new[] { -23.41430, -46.45475 },
            new[] { -23.41845, -46.47149 },
            new[] { -23.42136, -46.48867 },
            new[] { -23.42136, -46.50586 },
            new[] { -23.42177, -46.52305 },
            new[] { -23.42426, -46.54023 },
            new[] { -23.43090, -46.55742 },
            new[] { -23.44294, -46.57054 },
            new[] { -23.45870, -46.58229 },
            new[] { -23.46617, -46.59812 },
            new[] { -23.46576, -46.61531 },
            new[] { -23.47986, -46.62662 },
            new[] { -23.48650, -46.64290 },
            new[] { -23.48111, -46.66009 },
            new[] { -23.47073, -46.66687 },
            new[] { -23.46368, -46.68270 },
            new[] { -23.46202, -46.69989 },
            new[] { -23.45538, -46.71708 },
            new[] { -23.44128, -46.72703 },
            new[] { -23.42675, -46.73833 },
            new[] { -23.41306, -46.75054 },
            new[] { -23.39728, -46.75552 },
            new[] { -23.38151, -46.75597 },
            new[] { -23.37196, -46.74286 },
            new[] { -23.36947, -46.72567 },
            new[] { -23.37362, -46.70848 },
            new[] { -23.38026, -46.69175 },
            new[] { -23.38649, -46.67456 },
            new[] { -23.38732, -46.65737 },
            new[] { -23.37694, -46.64154 },
            new[] { -23.36199, -46.62798 },
            new[] { -23.34622, -46.61757 },
            new[] { -23.33168, -46.60400 },
            new[] { -23.32711, -46.58772 },
            new[] { -23.32545, -46.57054 },
            new[] { -23.32504, -46.55335 },
            new[] { -23.32171, -46.53616 },
            new[] { -23.31590, -46.51898 }
        };

        /// <summary>
        /// Verifica se um ponto (ray casting) está dentro do contorno da serra.
        /// </summary>
        public static bool Contem(double latitude, double longitude)
        {
            var dentro = false;
            var n = Contorno.Length;
            for (var i = 0; i < n; i++)
            {
                var j = (i + n - 1) % n;
                var yi = Contorno[i][0];
                var xi = Contorno[i][1];
                var yj = Contorno[j][0];
                var xj = Contorno[j][1];
                var cruza = (yi > latitude) != (yj > latitude) &&
                            longitude < (xj - xi) * (latitude - yi) / (yj - yi) + xi;
                if (cruza)
                {
                    dentro = !dentro;
                }
            }
            return dentro;
        }
    }
}
