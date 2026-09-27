using MacroDeck.Sdk.Migration;
using NUnit.Framework;

namespace hashashin.FxCommands.Tests;

[TestFixture]
public sealed class MigrationTests
{
    [Test]
    public void Does_not_claim_a_macro_deck_2_settings_source_that_it_cannot_migrate()
    {
        Assert.That(new MacroDeck2Migration().ClaimedSettingsSources, Is.Empty);
    }

    [Test]
    public async Task Migrates_the_legacy_action_configuration()
    {
        var action = new ForeignAction(
            "hashashin.FxCommands.Main+FxCommands",
            "hashashin.FxCommands",
            "Wave",
            "{\"command\":\"/e wave\"}",
            null);

        var result = await new MacroDeck2Migration()
            .MigrateActionAsync(action, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IntegrationId, Is.EqualTo("com.hashashin.fxcommands"));
        Assert.That(result.ActionId, Is.EqualTo("fx-command"));
        Assert.That(result.Label, Is.EqualTo("Wave"));
        Assert.That(result.Parameters["command"].GetString(), Is.EqualTo("/e wave"));
    }
}
