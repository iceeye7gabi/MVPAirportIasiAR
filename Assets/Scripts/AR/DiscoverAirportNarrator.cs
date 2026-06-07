using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Curated spoken facts about Aeroportul Internațional Iași for tap-to-listen in Discover mode.
    /// </summary>
    public class DiscoverAirportNarrator : MonoBehaviour
    {
        static readonly string[] FactsRo =
        {
            "Aeroportul Internațional Iași se află la aproximativ opt kilometri est de centrul orașului, pe șoseaua Moara de Foc.",
            "Codul IATA al aeroportului este IAS. Aeroportul deservește zboruri interne și internaționale din regiunea Moldovei.",
            "Din aeroport poți ajunge în centrul Iașiului cu taxi, ride-sharing sau transport public, în aproximativ douăzeci până la treizeci de minute.",
            "Aeroportul oferă zone de check-in, sală de așteptare, cafenea, magazine, toalete, Wi-Fi și parcare pentru pasageri.",
            "Aceasta este o aplicație demo pentru hackathon. Prezintă cum ar putea funcționa un ghid digital pentru pasagerii Aeroportului Iași.",
            "În această versiune demo, indicațiile din modul Descoperă sunt orientative: check-in în față, security la stânga, restaurant și toalete la dreapta."
        };

        int factIndex;

        public string GetNextFact()
        {
            string fact = FactsRo[factIndex % FactsRo.Length];
            factIndex++;
            return fact;
        }

        public void Reset()
        {
            factIndex = 0;
        }
    }
}
