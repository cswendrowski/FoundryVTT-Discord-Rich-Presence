(() => {
    const version = 1;  //Current Version
  
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

      var url = 'http://localhost:2324/api/PlayerStatus';
      var json = JSON.stringify(new PlayerStatus());
      var otherParams = {
        headers: {
          'Content-Type': 'application/json'
        },
        body: json,
        method: 'POST'
      };

      fetch(url, otherParams)
        .then(res => { console.log(res) })
        .catch(error =>
        {
          console.log(error);
          window.location = 'foundryvtt-richpresence://run';
        });
    }

    function doReplacements(input) {
      var replacements = [];
      var matches = input.match(/\[\[.*\]\]/g);
      if (!matches) return input;
      matches.forEach(x => replacements.push( { original : x, replacement : eval(x.replace("[[", "").replace("]]","")) } ));
      console.log(replacements);
      var replacedString = '';
      replacements.forEach(function(x) { replacedString = input.replace(x.original + '', x.replacement + ''); })

      return replacedString;
    }

    window.onbeforeunload = function()
    { 
      console.log("Discord Rich Presence | Unloading");
      var url = 'http://localhost:2324/api/PlayerStatus/leave';
      var otherParams = {
        method: 'POST'
      };

      fetch(url, otherParams)
        .then(res => { console.log(res) })
        .catch(error => console.log(error));

      sleep(100);
      
      console.log("Discord Rich Presence | Done!");
    }
  
    window.DiscordRichPresence.setup = () => {
      console.log(`Discord Rich Presence | Initializing v${version}`);

      Hooks.on('init', () => {
        game.settings.register('discord-rich-presence', 'whatTheGMIsCurrentlyDoingText', {
          name: 'The first line of text to display when the gamemaster is playing',
          hint: 'Can use [[game.X]] macros for dynamic values',
          scope: 'world',
          config: true,
          default: 'GMing',
          type: String,
        });
      });

      Hooks.on('init', () => {
        game.settings.register('discord-rich-presence', 'whatTheGMIsCurrentlyPlayingText', {
          name: 'The second line of text to display when the gamemaster is playing',
          hint: 'Can use [[game.X]] macros for dynamic values',
          scope: 'world',
          config: true,
          default: 'Playing [[game.system.data.title]]',
          type: String,
        });
      });

      Hooks.on('init', () => {
        game.settings.register('discord-rich-presence', 'whatThePlayerIsCurrentlyDoingText', {
          name: 'The text to display when the User has an active Character',
          hint: 'Can use [[game.X]] macros for dynamic values',
          scope: 'world',
          config: true,
          default: 'Playing as [[game.user.character.name]]',
          type: String,
        });
      });

      Hooks.on('init', () => {
        game.settings.register('discord-rich-presence', 'whatThePlayerIsCurrentlyDoingNoCharacterFoundText', {
          name: 'The text to display when the User has no assigned Character',
          hint: 'Can use [[game.X]] macros for dynamic values',
          scope: 'world',
          config: true,
          default: 'Playing [[game.system.data.title]]',
          type: String,
        });
      });

      Hooks.on('init', () => {
        game.settings.register('discord-rich-presence', 'whatThePartyIsCurrentlyDoingText', {
          name: 'The text to display for the Party\'s current status',
          hint: 'Can use [[game.X]] macros for dynamic values',
          scope: 'world',
          config: true,
          default: 'Exploring [[game.user.data.currentSceneName]]',
          type: String,
        });
      });

      Hooks.on('ready', () => {
        sendPlayerStatusUpdate();

        setInterval(function() { sendPlayerStatusUpdate(); }, 15 * 1000);
      });
    };
  })();
  