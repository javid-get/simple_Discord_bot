namespace simple_Discord_bot;

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;


/** <summary>
  Module for general-purpose commands.
</summary> */
public class UtilityModule : BaseCommandModule {
  /** <summary>
    Deletes a message; useful for cleaning deprecated messages.
  </summary> */
  [Command("del-msg")]
  public async Task DeleteMessage(CommandContext ctx, string messageId) {
    string[] split = messageId.Split('/');
    ulong id = ulong.Parse(split[split.GetUpperBound(0)]);
    DiscordMessage message = await ctx.Channel.GetMessageAsync(id);
    await ctx.Channel.DeleteMessageAsync(message);
  }
}
