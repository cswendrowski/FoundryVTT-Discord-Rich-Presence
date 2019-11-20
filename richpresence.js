(() => {
  const version = 2.0;  //Current Version
  var websocket = null;

  //Bootstrap
  if (!window.DiscordRichPresence) {
    window.DiscordRichPresence = {loaded: 0};
    window.DiscordRichPresence.setup = () => console.error('Discord Rich Presence | Failed to setup Discord Rich Presence');
    $(() => window.DiscordRichPresence.setup());
  }

  if (window.DiscordRichPresence.loaded >= version) {
    return;
  }
  window.DiscordRichPresence.loaded = version;

  function getCurrentSceneName() {
    var scenes = game.scenes.entities;
    var firstActiveScene = scenes.find(function(element) { return element._view; });

    if (!firstActiveScene) return "Unknown";

    return firstActiveScene.name;
  }

  function getSystemName() {
    return game.system.data.title;
  }

  function getCurrentPlayers() {
    return game.users.entities.filter(element => element.active ).length;
  }

  function getMaxPlayers() {
    return game.users.entities.length;
  }

  function getCurrentPlayerIsGm() {
    return game.user.isGM;
  }

  function getCurrentGameRemoteUrl() {
    return game.data.ips.remote;
  }

  function getUniqueWorldId() {
    return game.data.world.id;
  }

  function getSystemName() {
    return game.system.data.title;
  }

  function log(log) {
    if (game.settings.get("discord-rich-presence", "showDebugLogs")) {
      console.log(log);
    }
  }

  function getDetails() {
    if (getCurrentPlayerIsGm()) {
      return doReplacements(game.settings.get("discord-rich-presence", "whatTheGMIsCurrentlyDoingText"));
    }
    else if (game.user.character) {
      return doReplacements(game.settings.get("discord-rich-presence", "whatThePlayerIsCurrentlyDoingText"))
    }
    else {
      return doReplacements(game.settings.get("discord-rich-presence", "whatThePlayerIsCurrentlyDoingNoCharacterFoundText"))
    }
  }

  function getState() {
    if (getCurrentPlayerIsGm()) {
      return doReplacements(game.settings.get("discord-rich-presence", "whatTheGMIsCurrentlyPlayingText"));
    }
    else {
      return doReplacements(game.settings.get("discord-rich-presence", "whatThePartyIsCurrentlyDoingText"))
    }
  }

  class PlayerStatus {
    constructor()
    {
      this.Details = getDetails();
      this.State = getState();
      this.CurrentPlayerCount = getCurrentPlayers();
      this.MaxPlayerCount = getMaxPlayers();
      this.FoundryUrl = getCurrentGameRemoteUrl();
      this.WorldUniqueId = getUniqueWorldId();
      this.SystemName = getSystemName();
    }
  }

  // Gross
  function sleep(milliseconds) {
    var start = new Date().getTime();
    for (var i = 0; i < 1e7; i++) {
      if ((new Date().getTime() - start) > milliseconds){
        break;
      }
    }
  }

  function sendPlayerStatusUpdate() {
    game.user.data.currentSceneName = getCurrentSceneName();

    var json = JSON.stringify(new PlayerStatus());

    var msg = {
      Type: 4,
      Payload: json
    };

    websocket.send(JSON.stringify(msg));
  }

  function doReplacements(input) {
    log("Discord Rich Presence | Doing replacements for " + input);
    var replacements = [];
    var matches = input.match(/\[\[\S*\]\]/g);
    if (!matches) return input;
    matches.forEach(x => replacements.push( { original : x, replacement : eval(x.replace("[[", "").replace("]]","")) } ));
    log(replacements);
    var replacedString = '';
    replacements.forEach(function(x) { replacedString = input.replace(x.original + '', x.replacement + ''); })

    return replacedString;
  }

  function HandleDiscordProfileInfo(profileInfo) {
    game.user.setFlag("discord-rich-presence", "avatar", profileInfo.AvatarBase64);
    game.user.setFlag("discord-rich-presence", "discordId", profileInfo.DiscordId).then(function() {
      ui.players.render();
    });
    log("Registered current User's Discord Profile info");
  }

  function HandleVoiceStatus(voiceStatus) {
    //log(voiceStatus);

    game.user.setFlag("discord-rich-presence", "connected", voiceStatus.IsConnected).then(function() {
      ui.players.render();
    });

    game.user.setFlag("discord-rich-presence", "muted", voiceStatus.IsMuted).then(function() {
      log("Set muted to " + voiceStatus.IsMuted);
      ui.players.render();
    });

    game.user.setFlag("discord-rich-presence", "deafened", voiceStatus.IsDeafened).then(function() {
      log("Set deafened to " + voiceStatus.IsDeafened);
      ui.players.render();
    });
  }

  function HandleSpeaking(speaking) {
    if (speaking.IsSpeaking) {
      $("[data-discordid='" + speaking.DiscordId + "']").addClass("discord-speaking");
      FireOnSpeaking(speaking.DiscordId);
    }
    else {
      $("[data-discordid='" + speaking.DiscordId + "']").removeClass("discord-speaking");
      FireOnStopSpeaking(speaking.DiscordId);
    }
  }

  function AddInCallbar(html) {
    var toAdd = "<h3 class='discord-callbar'><svg name='Nova_CallLeave' id='DiscordLeaveVoice' class='flex-center' aria-hidden='false' width='18' height='18' viewBox='0 0 24 24'><path fill='currentColor' fill-rule='evenodd' clip-rule='evenodd' d='M21.1169 1.11603L22.8839 2.88403L19.7679 6.00003L22.8839 9.11603L21.1169 10.884L17.9999 7.76803L14.8839 10.884L13.1169 9.11603L16.2329 6.00003L13.1169 2.88403L14.8839 1.11603L17.9999 4.23203L21.1169 1.11603ZM18 22H13C6.925 22 2 17.075 2 11V6C2 5.447 2.448 5 3 5H7C7.553 5 8 5.447 8 6V10C8 10.553 7.553 11 7 11H6C6.063 14.938 9 18 13 18V17C13 16.447 13.447 16 14 16H18C18.553 16 19 16.447 19 17V21C19 21.553 18.553 22 18 22Z'></path></svg><div style='color: #43b581;' class='flex-center' >Voice Connected</div>";
    toAdd += "<div class='discord-voice-options'>";
    toAdd += Mute();
    toAdd += Deafen();
    toAdd += Options();
    toAdd += "</div>"
    html.find("#player-list").after(toAdd);
    $("#DiscordLeaveVoice").click(function() { LeaveVoice() });
    $("#DiscordVoiceOptions").click(function() { OpenVoiceSettings() });
    $("#DiscordMute").click(function() { MuteVoice() });
    $("#DiscordDeafen").click(function() { DeafenVoice() });
  }
  
  function AddOutOfCallbar(html) {
    html.find("#player-list").after('<h3 class="discord-callbar"><svg x="0" y="0" name="Nova_CallJoin" id="DiscordJoinVoice" class="flex-center" aria-hidden="false" width="18" height="18" viewBox="0 0 24 24" style="flex: 0 0 30px;"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M11 5V3C16.515 3 21 7.486 21 13H19C19 8.589 15.411 5 11 5ZM17 13H15C15 10.795 13.206 9 11 9V7C14.309 7 17 9.691 17 13ZM11 11V13H13C13 11.896 12.105 11 11 11ZM14 16H18C18.553 16 19 16.447 19 17V21C19 21.553 18.553 22 18 22H13C6.925 22 2 17.075 2 11V6C2 5.447 2.448 5 3 5H7C7.553 5 8 5.447 8 6V10C8 10.553 7.553 11 7 11H6C6.063 14.938 9 18 13 18V17C13 16.447 13.447 16 14 16Z"></path></svg><div style="color: #AAA; flex: 1 0 auto;" class="flex-center">Connect to Voice</div></h3>');
    $("#DiscordJoinVoice").click(function() { JoinVoice() });
  }

  function Options() {
    return '<svg name="Gear" class="flex-center" id="DiscordVoiceOptions" aria-hidden="false" width="20" height="20" viewBox="0 0 24 24"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M19.738 10H22V14H19.739C19.498 14.931 19.1 15.798 18.565 16.564L20 18L18 20L16.565 18.564C15.797 19.099 14.932 19.498 14 19.738V22H10V19.738C9.069 19.498 8.203 19.099 7.436 18.564L6 20L4 18L5.436 16.564C4.901 15.799 4.502 14.932 4.262 14H2V10H4.262C4.502 9.068 4.9 8.202 5.436 7.436L4 6L6 4L7.436 5.436C8.202 4.9 9.068 4.502 10 4.262V2H14V4.261C14.932 4.502 15.797 4.9 16.565 5.435L18 3.999L20 5.999L18.564 7.436C19.099 8.202 19.498 9.069 19.738 10ZM12 16C14.2091 16 16 14.2091 16 12C16 9.79086 14.2091 8 12 8C9.79086 8 8 9.79086 8 12C8 14.2091 9.79086 16 12 16Z"></path></svg>';
  }

  function Mute() {
    return '<svg name="Nova_Microphone" class="flex-center" id="DiscordMute" aria-hidden="false" width="20" height="20" viewBox="0 0 24 24"><path fill-rule="evenodd" clip-rule="evenodd" d="M14.99 11C14.99 12.66 13.66 14 12 14C10.34 14 9 12.66 9 11V5C9 3.34 10.34 2 12 2C13.66 2 15 3.34 15 5L14.99 11ZM12 16.1C14.76 16.1 17.3 14 17.3 11H19C19 14.42 16.28 17.24 13 17.72V21H11V17.72C7.72 17.23 5 14.41 5 11H6.7C6.7 14 9.24 16.1 12 16.1ZM12 4C11.2 4 11 4.66667 11 5V11C11 11.3333 11.2 12 12 12C12.8 12 13 11.3333 13 11V5C13 4.66667 12.8 4 12 4Z" fill="currentColor"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M14.99 11C14.99 12.66 13.66 14 12 14C10.34 14 9 12.66 9 11V5C9 3.34 10.34 2 12 2C13.66 2 15 3.34 15 5L14.99 11ZM12 16.1C14.76 16.1 17.3 14 17.3 11H19C19 14.42 16.28 17.24 13 17.72V22H11V17.72C7.72 17.23 5 14.41 5 11H6.7C6.7 14 9.24 16.1 12 16.1Z" fill="currentColor"></path></svg>';
  }

  function Deafen() {
    return '<svg name="Nova_Headset" class="flex-center" id="DiscordDeafen" aria-hidden="false" width="20" height="20" viewBox="0 0 24 24"><svg width="24" height="24" viewBox="0 0 24 24"><path d="M12 2.00305C6.486 2.00305 2 6.48805 2 12.0031V20.0031C2 21.1071 2.895 22.0031 4 22.0031H6C7.104 22.0031 8 21.1071 8 20.0031V17.0031C8 15.8991 7.104 15.0031 6 15.0031H4V12.0031C4 7.59105 7.589 4.00305 12 4.00305C16.411 4.00305 20 7.59105 20 12.0031V15.0031H18C16.896 15.0031 16 15.8991 16 17.0031V20.0031C16 21.1071 16.896 22.0031 18 22.0031H20C21.104 22.0031 22 21.1071 22 20.0031V12.0031C22 6.48805 17.514 2.00305 12 2.00305Z" fill="currentColor"></path></svg></svg>';
  }

  function Muted() {
    return "<div aria-label='Muted' style='flex: 0 0 12px;'><svg name='Nova_MicrophoneMute' class='icon-3BcwQu' aria-hidden='false' viewBox='0 0 24 24' style='flex: 0 0 18px;' width='18' height='18'><path d='M6.7 11H5C5 12.19 5.34 13.3 5.9 14.28L7.13 13.05C6.86 12.43 6.7 11.74 6.7 11Z' fill='currentColor'></path><path d='M9.01 11.085C9.015 11.1125 9.02 11.14 9.02 11.17L15 5.18V5C15 3.34 13.66 2 12 2C10.34 2 9 3.34 9 5V11C9 11.03 9.005 11.0575 9.01 11.085Z' fill='currentColor'></path><path d='M11.7237 16.0927L10.9632 16.8531L10.2533 17.5688C10.4978 17.633 10.747 17.6839 11 17.72V22H13V17.72C16.28 17.23 19 14.41 19 11H17.3C17.3 14 14.76 16.1 12 16.1C11.9076 16.1 11.8155 16.0975 11.7237 16.0927Z' fill='currentColor'></path><path d='M21 4.27L19.73 3L3 19.73L4.27 21L8.46 16.82L9.69 15.58L11.35 13.92L14.99 10.28L21 4.27Z' fill='currentColor'></path></svg></div>";
  }

  function Deafened() {
    return "<div aria-label='Deafened'><svg name='Nova_HeadsetDeafen' class='icon-3BcwQu' aria-hidden='false' viewBox='0 0 24 24' width='18' height='18'><path d='M6.16204 15.0065C6.10859 15.0022 6.05455 15 6 15H4V12C4 7.588 7.589 4 12 4C13.4809 4 14.8691 4.40439 16.0599 5.10859L17.5102 3.65835C15.9292 2.61064 14.0346 2 12 2C6.486 2 2 6.485 2 12V19.1685L6.16204 15.0065Z' fill='currentColor'></path><path d='M19.725 9.91686C19.9043 10.5813 20 11.2796 20 12V15H18C16.896 15 16 15.896 16 17V20C16 21.104 16.896 22 18 22H20C21.105 22 22 21.104 22 20V12C22 10.7075 21.7536 9.47149 21.3053 8.33658L19.725 9.91686Z' fill='currentColor'></path><path d='M3.20101 23.6243L1.7868 22.2101L21.5858 2.41113L23 3.82535L3.20101 23.6243Z' fill='currentColor'></path></svg></div>";
  }

  function MuteVoice() {
    var payload = {
      IsMuted: true
    };
  
    var msg = {
      Type: 5,
      Payload: JSON.stringify(payload)
    };
  
    websocket.send(JSON.stringify(msg));
  }

  function DeafenVoice() {
    var payload = {
      IsDeafened: true
    };
  
    var msg = {
      Type: 5,
      Payload: JSON.stringify(payload)
    };
  
    websocket.send(JSON.stringify(msg));
  }
  
  function JoinVoice() {
    var payload = {
      ShouldConnect: true,
      VoicePartySize: getMaxPlayers(),
      WorldUniqueIdentifier: game.data.ips.remote
    };
  
    var msg = {
      Type: 6,
      Payload: JSON.stringify(payload)
    };
  
    websocket.send(JSON.stringify(msg));
  }
  
  function LeaveVoice() {
    if (websocket) {
      var payload = {
        ShouldConnect: false
      };
    
      var msg = {
        Type: 6,
        Payload: JSON.stringify(payload)
      };
    
      websocket.send(JSON.stringify(msg));
    }

    game.user.setFlag("discord-rich-presence", "connected", false).then(function() {
      ui.players.render();
    });
  }

  function OpenVoiceSettings() {
    var msg = {
      Type: 7,
      Payload: ""
    };
  
    websocket.send(JSON.stringify(msg));
  }
  
  function FireOnSpeaking(discordId) {
    Hooks.callAll("discord-userspeaking", discordId, true);
  }
  
  function FireOnStopSpeaking(discordId) {
    Hooks.callAll("discord-userspeaking", discordId, false);
  }

  window.DiscordRichPresence.setup = () => {
    console.log(`Discord Rich Presence | Initializing v` + version);

    RegisterConfigurationOptions();

    Hooks.on('ready', () => {
      if (!game.settings.get("discord-rich-presence", "enabled")) {
        console.log("\"Enabled for this client\" option is currently False, module is exitting without doing anything!");
        return;
      }

      LeaveVoice()

      websocket = new ReconnectingWebSocket("ws://127.0.0.1:2324/Status");
    
      websocket.onmessage = function (event) {

        var msg = JSON.parse(event.data);
        log(msg);

        var payload = JSON.parse(msg.Payload);

        switch (msg.Type) {
          case 3: HandleDiscordProfileInfo(payload); break;
          case 5: HandleVoiceStatus(payload); break;
          case 8: HandleSpeaking(payload); break;
        }
      }

      websocket.onerror = function (event) {
        LeaveVoice();
        if (game.settings.get("discord-rich-presence", "handleLocalApiLifecycle")) {
          window.open('foundryvtt-richpresence://run');
        }
      }

      websocket.onopen = function (event) {

        var payload = {
          FoundryUserId: game.userId
        };

        var msg = {
          Type: 1,
          Payload: JSON.stringify(payload)
        };

        websocket.send(JSON.stringify(msg));

        sendPlayerStatusUpdate(websocket);

        setInterval(function() { sendPlayerStatusUpdate(); }, 30 * 1000);

        if (game.settings.get("discord-rich-presence", "autojoinVoice")) {
          JoinVoice();
        }
      }

      window.onbeforeunload = function()
      { 
        console.log("Discord Rich Presence | Unloading");

        var payload = {
          ShouldExit: game.settings.get("discord-rich-presence", "handleLocalApiLifecycle")
        }

        var msg = {
          Type: 2,
          Payload: JSON.stringify(payload)
        };

        websocket.send(JSON.stringify(msg));
        websocket.close();
  
        sleep(100);
        
        console.log("Discord Rich Presence | Done!");
      }

      log(websocket);

      // Thanks Atropos
      Hooks.on("updateUser", (user, data, options, userId) => {
        if (
          hasProperty(data, "flags.discord-rich-presence.avatar") ||
          hasProperty(data, "flags.discord-rich-presence.muted") ||
          hasProperty(data, "flags.discord-rich-presence.deafened") ||
          hasProperty(data, "flags.discord-rich-presence.connected") ||
          hasProperty(data, "flags.discord-rich-presence.connected") ||
          hasProperty(data, "flags.discord-rich-presence.discordId")
        )
        {
          log("Another player's status updated, rerendering player list")
          ui.players.render();
        }
      });

      Hooks.on("renderPlayerList", (app, html, data) => {

        data.users.forEach((user) => {

          var avatar = user.getFlag("discord-rich-presence", "avatar");
          var muted = user.getFlag("discord-rich-presence", "muted");
          var deafened = user.getFlag("discord-rich-presence", "deafened");
          var connected = user.getFlag("discord-rich-presence", "connected");
          var discordId = user.getFlag("discord-rich-presence", "discordId");

          var playerLi = html.find("[data-user-id='" + user.id + "']");
          $(playerLi).attr("data-discordId", '' + discordId);

          if (connected) {

            var playerActive =  playerLi.find(".player-active");

            playerActive.each((i, span) => {
              $(span).replaceWith("<img class='discord-avatar' src='" + avatar + "' />")
            });
            
            playerLi.find(".player-name").each((i, span) => {
              var muteInfo = "<div class='discord-muteinfo'>";

              if (muted) {
                muteInfo = muteInfo + Muted();
              }

              if (deafened) {
                muteInfo = muteInfo + Deafened();
              }

              $(span).after(muteInfo + "</div>");
            });
          }
        });

        if (game.user.getFlag("discord-rich-presence", "connected"))
        {
          AddInCallbar(html);
        }
        else
        {
          AddOutOfCallbar(html);
        }
      });
    });
  };
})();

function RegisterConfigurationOptions() {

Hooks.on('init', () => {

  game.settings.register('discord-rich-presence', 'whatTheGMIsCurrentlyDoingText', {
    name: 'The first line of text to display when the gamemaster is playing',
    hint: 'Can use [[game.X]] syntax for dynamic values',
    scope: 'world',
    config: true,
    default: 'GMing',
    type: String,
  });

  game.settings.register('discord-rich-presence', 'whatTheGMIsCurrentlyPlayingText', {
    name: 'The second line of text to display when the gamemaster is playing',
    hint: 'Can use [[game.X]] syntax for dynamic values',
    scope: 'world',
    config: true,
    default: 'Setting up [[game.system.data.title]]',
    type: String,
  });

  game.settings.register('discord-rich-presence', 'whatThePlayerIsCurrentlyDoingText', {
    name: 'The text to display when the User has an active Character',
    hint: 'Can use [[game.X]] syntax for dynamic values',
    scope: 'world',
    config: true,
    default: 'Playing as [[game.user.character.name]]',
    type: String,
  });

  game.settings.register('discord-rich-presence', 'whatThePlayerIsCurrentlyDoingNoCharacterFoundText', {
    name: 'The text to display when the User has no assigned Character',
    hint: 'Can use [[game.X]] syntax for dynamic values',
    scope: 'world',
    config: true,
    default: 'Playing [[game.system.data.title]]',
    type: String,
  });

  game.settings.register('discord-rich-presence', 'whatThePartyIsCurrentlyDoingText', {
    name: 'The text to display for the Party\'s current status',
    hint: 'Can use [[game.X]] syntax for dynamic values',
    scope: 'world',
    config: true,
    default: 'Exploring [[game.user.data.currentSceneName]]',
    type: String,
  });

  game.settings.register('discord-rich-presence', 'showDebugLogs', {
    name: 'Show debug logs?',
    hint: 'Turn this on for bug reports',
    scope: 'world',
    config: true,
    default: false,
    type: Boolean,
  });

  game.settings.register('discord-rich-presence', 'handleLocalApiLifecycle', {
    name: 'Should the module handle the startup / shutdown of the companion api?',
    hint: 'Turn this off if you are always running the API locally (say, as a background service or startup service)',
    scope: 'client',
    config: true,
    default: true,
    type: Boolean,
  });

  game.settings.register('discord-rich-presence', 'autojoinVoice', {
    name: 'Auto Join Voice?',
    hint: 'Turn this on if you want to automatically connect to voice when connected to Discord',
    scope: 'client',
    config: true,
    default: false,
    type: Boolean,
  });

  game.settings.register('discord-rich-presence', 'enabled', {
    name: 'Enabled for this client',
    hint: '!!! Turn this on if you want to use this module!!!',
    scope: 'client',
    config: true,
    default: false,
    type: Boolean,
  });
});
}
