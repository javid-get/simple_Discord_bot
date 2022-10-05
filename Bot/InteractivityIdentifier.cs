namespace simple_Discord_bot;

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.EventArgs;


/** <summary>
  Interactable components in the Discord UI have identifiers to tell them apart.
  This class models an indentifer, and helps to create components with specific identifiers: buttons in particular.
</summary> */
public class InteractivityIdentifier {
  public const char Delimiter = '_';
  public readonly ulong hostId;
  public readonly ulong playerId;
  public readonly Type type;
  public InteractivityIdentifier(ulong hostId, ulong playerId, Type type) {
    this.hostId = hostId;
    this.playerId = playerId;
    this.type = type;
  }

  public Games.Main.Board.Direction Direction {
    get {
      switch (type) {
        case Type.NorthButton: return Games.Main.Board.Direction.North;
        case Type.SouthButton: return Games.Main.Board.Direction.South;
        case Type.EastButton: return Games.Main.Board.Direction.East;
        case Type.WestButton: return Games.Main.Board.Direction.West;
        default: return Games.Main.Board.Direction.Null;
      }
    }
  }

  public string Label {
    get {
      switch (type) {
        case Type.NorthButton: return "North";
        case Type.SouthButton: return "South";
        case Type.EastButton: return "East";
        case Type.WestButton: return "West";
        default: return "Refresh";
      }
    }
  }

  public InteractivityIdentifier(string idStr) {
    string[] args = idStr.Split(Delimiter);
    this.hostId = ulong.Parse(args[1]);
    this.type = (Type) byte.Parse(args[2]);
    this.playerId = ulong.Parse(args[3]);
  }

  private static string createButtonId(ulong hostId, Type type, ulong memberId) {
    char d = Delimiter;
    return $"{((byte)Games.GameType.Main)}{d}{hostId}{d}{((byte)type)}{d}{memberId}";
  }

  public override string ToString() {
    return createButtonId(hostId, type, playerId);
  }

  public DiscordButtonComponent createButton() {
    return new DiscordButtonComponent(ButtonStyle.Primary, this.ToString(), Label);
  }

  public enum Type : byte {
    NorthButton, SouthButton, EastButton, WestButton, RefreshButton
  }
}
