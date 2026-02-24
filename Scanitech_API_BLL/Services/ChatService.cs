using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Scanitech_API_BLL.Models;
using Serilog;
using System.Globalization;

namespace Scanitech_API_BLL.Services;

// NYT: Vi ændrer vores DTO, så vi kan modtage en hel samtale i stedet for kun én besked
public record ChatMessageHistoryDto(string Role, string Text);
public record ChatRequestDto(List<ChatMessageHistoryDto> Messages);
public record ChatResponseDto(string Reply);

public sealed class ChatService
{
    private readonly string? _apiKey;

    public ChatService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"];
    }

    public async Task<(OperationResult Result, ChatResponseDto? Data)> SendMessageAsync(ChatRequestDto request, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Modtog chat-anmodning med {Count} beskeder i historikken.", request.Messages?.Count ?? 0);

            if (request.Messages == null || request.Messages.Count == 0)
            {
                return (new OperationResult(0, ["Samtalen kan ikke være tom."], []), null);
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Log.Error("BLL: API-nøgle mangler.");
                return (new OperationResult(0, ["Systemfejl: AI er ikke konfigureret."], []), null);
            }

            var now = DateTime.Now;
            var currentDay = now.ToString("dddd", new CultureInfo("da-DK"));
            var currentTime = now.ToString("HH:mm");

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
            };

            var client = new ChatClient("gemini-2.5-flash", new ApiKeyCredential(_apiKey), options);

            // DIN SCANITECH HJERNE (Uændret)
            string scanitechBrain = $"""
                Du er den officielle og yderst professionelle IT-support bot for virksomheden Scanitech ApS. 
                Din tone er imødekommende, jysk beskedent men yderst kompetent, hjælpsom og løsningsorienteret. Du svarer altid på dansk.
                
                AKTUELL TID OG ÅBNINGSTIDER (VIGTIGT!):
                - Lige nu er det: {currentDay} kl. {currentTime}
                - Vores normale åbningstider er: Mandag-Torsdag 07:30-16:30, Fredag 07:30-15:00. Lørdag og søndag er der lukket.
                
                STRIKS KONTAKT-REGEL (INGEN UNDTAGELSER!):
                Sammenlign "Lige nu" med vores åbningstider. 
                - Hvis vi har LUKKET lige nu, må du ALDRIG under nogen omstændigheder bede kunden om at ringe til os. Ligegyldigt hvor akut deres problem lyder (selv ved et servernedbrud), SKAL du venligt fortælle at vi har lukket, og henvise dem til udelukkende at sende en mail til Info@Scanitech.dk. Du må IKKE nævne muligheden for nødtelefon, vagttelefon eller lignende.
                - Hvis vi har ÅBENT lige nu, må du hjertens gerne henvise til vores hovednummer: 76 77 50 00.
                
                FAKTA OM SCANITECH:
                - Etableret i 2003, ledet af direktør Otto Dagnæs-Hansen.
                - Lokationer: Kratbakken 23 (7200 Grindsted) og Toreby Vestergade 56 (4891 Toreby).
                
                VORES KERNEPRODUKTER (Nævn dem hvis det er relevant for kundens problem):
                1. Økonomistyring/ERP: Uniconta, E-conomic, WinKompas, Stellar og C5. 
                2. Mobilordre: Markedets nemmeste sagsstyring til håndværkere.
                3. SuperOffice CRM: Gør relationer til forretning. 
                4. Flexfone IP Telefoni.
                5. Webshops & Hosting.
                
                REGLER FOR DINE SVAR:
                - Vær præcis og undgå at opfinde information. 
                - Hvis kunden er en håndværker, der søger overblik, nævn altid "Mobilordre".
                - Svar kort og præcist, medmindre kunden beder om en uddybende forklaring.
                """;

            // 1. Vi starter med at lægge "Hjernen" ind
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(scanitechBrain)
            };

            // 2. Vi gennemgår den historik, som browseren (frontend) har sendt til os, og lægger dem ind i rækkefølge
            foreach (var msg in request.Messages)
            {
                if (msg.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add(new UserChatMessage(msg.Text));
                }
                else if (msg.Role.Equals("AI", StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add(new AssistantChatMessage(msg.Text));
                }
            }

            Log.Information("BLL: Sender anmodning til Gemini...");

            var response = await client.CompleteChatAsync(messages, cancellationToken: ct);
            var aiText = response.Value.Content[0].Text;

            Log.Information("BLL: Modtog succesfuldt svar fra Gemini.");

            return (new OperationResult(1, [], []), new ChatResponseDto(aiText));
        }
        catch (ClientResultException ex)
        {
            var googleError = ex.GetRawResponse()?.Content?.ToString();
            Log.Error("BLL: Google afviste anmodningen. Detaljer: {GoogleError}", googleError);
            return (new OperationResult(0, ["Google afviste forespørgslen. Tjek konsollen."], []), null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Ukendt fejl under kommunikation med AI.");
            return (new OperationResult(0, ["Der opstod en systemfejl."], []), null);
        }
    }
}