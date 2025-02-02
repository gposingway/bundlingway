using Bundlingway.Model;

namespace Bundlingway
{
    public static class Constants
    {
        public static List<string> TextureExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".dds"];
        public static List<string> ShaderExtensions { get; set; } = [".fx", ".fxh"];
        public static List<string> InstallableExtensions { get; set; } = [".zip", ".rar", ".7z", ".ini"];
        public static ResourcePackage SingleFileCatalog { get; set; } = new ResourcePackage()
        {
            LocalPresetFolder = Path.Combine(Instances.SinglePresetsFolder, WellKnown.PresetFolder),
            Name = "Single Presets",
            Source = "Local",
            Type = "Single",
            Status = "Installed",
            Default = true,
            Installed = true,
            Hidden = true,
        };

        public static string AppUserModelId = "BundlingwayPackageManager";

        public static class Events
        {
            public static string PackageInstalled = "package-installed";
        }
        public static class WellKnown
        {
            public static readonly string ShaderFolder = "Shaders";
            public static readonly string PresetFolder = "Presets";
            public static readonly string TextureFolder = "Textures";
            public static readonly string CatalogEntryFile = "catalog-entry.json";
            public static readonly string GPosingwayConfigFileName = "gposingway-definitions.json";
            public static readonly string PackagesFolder = "Packages";
            public static readonly string CacheFolder = "Cache";
            public static readonly string ConfigFileName = "config.json";
            public static readonly string TempFolderName = "temp";
            public static readonly string LogFileName = "bundlingway-log.txt";
            public static readonly string SinglePresetsFolder = "Single Presets";

            public static readonly string GamePresetsFolder = "reshade-presets";
            public static readonly string GameShadersFolder = "reshade-shaders";
            internal static readonly string SinglePresetFile = "Single Preset";
        }

        public static BundlingwayDialogueOptions Bundlingway = new();

        public enum MessageCategory
        {
            ApplicationStart,
            Ready,
            AddPackage,
            RemovePackage,
            ReinstallPackage,
            UninstallPackage,
            Finished,
            BeforeAddPackageSelection,
            AddPackageSelectionCancelled,
            DetectingSettings,
            GenericDone,
            IdleCommentary
        }

        public class BundlingwayDialogueOptions
        {
            private readonly Random _random = new Random();
            private readonly Dictionary<MessageCategory, List<string>> _messages;

            public BundlingwayDialogueOptions()
            {
                _messages = new Dictionary<MessageCategory, List<string>>
            {
                {
                    MessageCategory.ApplicationStart, new List<string>
                    {
                        "Bundlingway, at your service! Let's get those ReShade presets organized, shall we?",
                        "Hop, hop! Bundlingway is starting up! Prepare for preset perfection!",
                        "Dreaming of beautiful pixels? Bundlingway is here to help!",
                        "Good morning! Or is it evening? Bundlingway doesn't judge, as long as we're bundling!",
                        "Behold, Bundlingway is booting! Let's make those games look *stunning*!",
                        "Carrots ready? Bundles primed? Let's do this, Bundlingway is online!",
                        "Don't take booting up Bundlingway too seriously, it's gonna finish in about a third of a second!"
                    }
                },
                {
                    MessageCategory.Ready, new List<string>
                    {
                        "Bundlingway is ready to roll! What preset magic shall we weave today?",
                        "All systems go! Bundlingway is at your command!",
                        "Ready and raring to go! Let's manage those bundles!",
                        "Bundlingway is online and feeling fine! Let the preset party begin!",
                        "I was born to bundle, and bundle shall I do!",
                        "Bundlingway is prepped and ready! What wonders will you create?",
                        "Hopefully this has been a help... Bundlingway is ready for action!",
                        "Hells he's every bit as coded and compiled as I remembered. Bundlingway is ready."
                    }
                },
                {
                    MessageCategory.AddPackage, new List<string>
                    {
                        "Package {1} of {0}, {2}!  Just a little hop, skip, and a jump!",
                        "Package {1} of {0}, {2}, is joining the party!  Let's make it feel welcome!",
                        "Ooh, shiny new package! Adding {2}, {1} of {0} now!",
                        "Carefully adding package {1} of {0}, {2}... Don't want to drop it!",
                        "Package {1} of {0}, {2}, is being added now!",
                        "Package {1} of {0}... The most important lesson I've learned is - WAIT!",
                        "Wuk Lamat is also in package {1} of {0}, there's no escape from people worshipping her.",
                        "Package {1} of {0}... Your ReShade setup is the perfect home for {2}!"
                    }
                },
                {
                    MessageCategory.RemovePackage, new List<string>
                    {
                        "Time to say goodbye! Removing this package with a gentle nudge...",
                        "Don't worry, this package is just going on a little vacation. Removing it now!",
                        "Poof! Removing the package now. It'll be like it was never there!",
                        "Carefully extracting the package... We wouldn't want to leave any crumbs behind!",
                        "Even the most beloved packages sometimes need to make way for others. Removing now...",
                        "I hope they find their way, surely there's a folder out there for everybody. Removing package...",
                        "Removing package... I'm every bit as muscular and removing as I remembered.",
                        "They got nothing on you... but we're still removing this package.",
                        "They got something on you... so we're removing this package."
                    }
                },
                {
                    MessageCategory.ReinstallPackage, new List<string>
                    {
                        "Giving this package a fresh start! Reinstalling now!",
                        "Time for a makeover! Reinstalling this package to make it shine!",
                        "Let's try that again, shall we? Reinstalling the package now...",
                        "A little hiccup, eh? No worries, we're reinstalling!",
                        "Reinstalling... because sometimes, even the best presets need a little do-over.",
                        "Reinstalling package... Hopefully, this has been a help...",
                        "Reinstalling... I am every bit as muscular and reinstalling as I remembered.",
                        "Reinstalling... they got nothing on you... but maybe they do, we'll see!"
                    }
                },
                {
                    MessageCategory.UninstallPackage, new List<string>
                    {
                        "This package is off to a new adventure! Uninstalling now...",
                        "Saying a fond farewell! Uninstalling this package!",
                        "Making space for new possibilities! Uninstalling now...",
                        "Time for a clean slate! Uninstalling this package.",
                        "Uninstalling... It's not goodbye, it's 'see you later!'",
                        "Uninstalling package... Don't worry, it's going to a better place... probably.",
                        "Uninstalling... I am every bit as muscular and uninstalling as I remembered.",
                        "Uninstalling... they got nothing on you... but you've got something on your drive, so off it goes!"
                    }
                },
                {
                    MessageCategory.Finished, new List<string>
                    {
                        "Ta-da! All done!",
                        "Mission accomplished! What's next on our preset agenda?",
                        "And that's a wrap! Bundlingway is ready for more!",
                        "Success! Your ReShade setup thanks you!",
                        "Huzzah! We did it!",
                        "Frankly, you've exceeded my expectations. All done!",
                        "I feel so much better knowing you got that done. Finished!",
                        "There are still plenty of operations to do, but that one is finished!"
                    }
                },
                {
                    MessageCategory.BeforeAddPackageSelection, new List<string>
                    {
                        "So many packages to choose from! Where to begin?",
                        "Let's find some shiny new presets to add!",
                        "Time to expand your ReShade horizons! Which packages will you choose?",
                        "Ready to browse the selection? Let's see what treasures we can find!",
                        "Looking to add some new packages? I've got a good feeling about this!",
                        "I love this! What package do you want to add?"
                    }
                },
                {
                    MessageCategory.AddPackageSelectionCancelled, new List<string>
                    {
                        "No worries, we can always add packages later!",
                        "Changed your mind? That's okay!",
                        "Maybe another time! Bundlingway will be here when you're ready.",
                        "Alright, let's explore other options then!",
                        "That's the way it goes sometimes. Onward and upward!"
                    }
                },

                {
                    MessageCategory.DetectingSettings, new List<string>
                    {
                        "Sniffing out your game folder... I hope it smells nice!",
                        "Checking under the hood... Let's see what versions we've got!",
                        "Peeking into your settings... Don't mind me, just making sure everything's in order!",
                        "Bundlingway is on the case! Detecting your ReShade and GPosingway versions...",
                        "Let's make sure you have the right setup for the best ReShade experience!"
                    }
                },
                {
                    MessageCategory.GenericDone, new List<string>
                    {
                        "All done! What's next?",
                        "Finished! Bundlingway is ready for another task.",
                        "There we go! Everything's sorted.",
                        "That's that! What shall we do now?",
                        "Completed! Your setup is looking great.",
                        "Another one bites the dust! Or rather, another task bites the dust and is completed!"
                    }
                },
      {
                    MessageCategory.IdleCommentary, new List<string>
                    {
                        "Hmm, what to do...",
                        "Wonder what Dreamingway is dreaming about...",
                        "Carrots are good for the eyes, and for ReShade presets, apparently!",
                        "Clouds look like fluffy bundles of presets today!",
                        "Did you know Loporrits are excellent dancers? It's all in the hops.",
                        "If a tree falls in the forest, does it make a preset?",
                        "The moon's a nice place to live, but the commute to Eorzea is looong.",
                        "No 'I' in team, but there is an 'I' in 'Bundlingway'!",
                        "I'm a pretty good singer. Want to hear my 'La-hee'? (Clears throat)",
                        "The word 'bed' actually looks like a bed!",
                        "A Loporrit's superpower wish? Teleporting carrots to their mouth!",
                        "A carrot a day keeps the doctor away, unless the doctor is a ReShade enthusiast!",
                        "Life is short, eat carrots and install ReShade presets!",
                        "I should take up knitting. Tiny sweaters for all the presets!",
                        "Why did the scarecrow win an award? He was outstanding in his field!",
                        "Restaurant on the moon? Good food, but no atmosphere!",
                        "What do you call a Loporrit musician? A hop-era singer!",
                        "Carrots or tweaking ReShade settings... both are addictive!",
                        "Do people on the ground look up and say 'Look, it's the Loporrits!'?",
                        "Sometimes I wonder if the stars are just really, really tiny Loporrits.",
                        "We Loporrits are philosophers. We ponder big questions like 'What's the meaning of life?' and 'Where's that carrot?'",
                        "What's a Loporrit's favorite music? Hip-hop!",
                        "My book's title? 'One Small Hop for a Loporrit, One Giant Leap for Loporrit-kind.'",
                        "Didn't succeed? Try doing it the way Knowingway told you to.",
                        "We built this place while being 4 carrots tall. Impressive, right?",
                        "Life gives you lemons, make lemonade. Life gives you carrots? Make carrot juice! Carrot cake!",
                        "I saw a cloud that looked like a carrot today. It was a sign!",
                        "What's a Loporrit's favorite game? Hopscotch!",
                        "Why did the Loporrit bring a ladder to the bar? Drinks were on the house!",
                        "A good stretch or a successful ReShade install... both are satisfying!",
                        "We Loporrits are good at keeping secrets. No one can hear us whisper!",
                        "It's amazing we can talk to Eorzeans, and they understand us!",
                        "A Loporrit's work is never done, especially with ReShade presets to manage!",
                        "Is a ReShade preset worth a thousand words? Let's find out!",
                        "I'm on a seafood diet. I see food, I eat it, especially carrots!",
                        "Remember, a balanced diet is a carrot in each hand.",
                        "I asked for a Loporrit to help, but this is also fine!",
                        "The best things in life are free. Like air, love, and organized ReShade presets.",
                        "Bundlingway is my name, managing ReShade presets is my game. Literally.",
                        "A good ReShade preset is good, but a good friend to share it with is better.",
                        "Laughter is the best medicine. But a good ReShade setup is therapeutic.",
                        "Life is like a box of chocolates. Unless it's ReShade presets, then it's awesome.",
                        "The early bird gets the worm. The second mouse gets the cheese. The Loporrit gets the ReShade preset.",
                        "I'd be rich if I had a gil for every ReShade preset thought.",
                        "Be the change you want to see in the world, or in your ReShade preset.",
                        "You miss 100% of the shots you don't take, and 100% of presets you don't install.",
                        "You can't have your cake and eat it too. But you can have your preset and use it too.",
                        "Don't count your chocobos before they hatch. But do count your ReShade presets.",
                        "When in Rome, do as the Romans. In Eorzea, as the Eorzeans. Using Bundlingway, as the Loporrits, and install presets.",
                        "If life gives you lemons, make lemonade. If life gives you ReShade, make your game beautiful.",
                        "A picture is worth a thousand words. A good ReShade preset is worth more.",
                        "Don't put off until tomorrow what you can do today, unless it's installing presets - let me do that for you!",
                        "A journey of a thousand miles begins with a single step, and a perfect ReShade setup with a click."
                    }
                }
            };
            }

            public string GetMessage(MessageCategory category, params string[] args)
            {
                if (_messages.TryGetValue(category, out List<string> messages))
                {
                    string messageTemplate = messages[_random.Next(messages.Count)];

                    return string.Format(messageTemplate, args);
                }

                // Return a default message if the category is not found (shouldn't happen in normal usage)
                return "Bundlingway is at a loss for words!";
            }

            public async Task<string> GetMessageAsync(MessageCategory category, params string[] args)
            {
                return await Task.Run(() =>
                {
                    if (_messages.TryGetValue(category, out List<string> messages))
                    {
                        string messageTemplate = messages[_random.Next(messages.Count)];

                        if (category == MessageCategory.AddPackage && args.Length == 2)
                        {
                            return string.Format(messageTemplate, args);
                        }
                        else
                        {
                            return messageTemplate;
                        }
                    }

                    // Return a default message if the category is not found (shouldn't happen in normal usage)
                    return "Bundlingway is at a loss for words!";
                });
            }

        }
    }
}