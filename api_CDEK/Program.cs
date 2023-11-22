// account: IdIejLzCOIocgZjkdC2yBb4Scs6tVImq
// secret_key: Qtwdni8MbMMwOu6oKZGtMlzK55QNmx4Z
//grant_type: client_credentials

using api_CDEK;

var client = new ClientCDEK("IdIejLzCOIocgZjkdC2yBb4Scs6tVImq", "Qtwdni8MbMMwOu6oKZGtMlzK55QNmx4Z");
Console.WriteLine(await client.CalculatePrice("Ижевск", 12, 0.07));

