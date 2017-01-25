using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroWizard;
using Google.Protobuf.Collections;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;

namespace PoGo.NecroBot.Logic.Forms
{
    public partial class InitialTutorialForm : System.Windows.Forms.Form
    {
        private ISession session;
        private RepeatedField<TutorialState> tutState;
        CheckTosState state;
        private EncounterTutorialCompleteResponse encounterTutorialCompleteResponse;

        public InitialTutorialForm()
        {
            InitializeComponent();
        }

        public InitialTutorialForm(CheckTosState s, RepeatedField<TutorialState> tutState, ISession session)
        {
            InitializeComponent();
            this.state = s;
            this.tutState = tutState;
            this.session = session;
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void wizardPage4_Initialize(object sender, WizardPageInitEventArgs e)
        {
            Task.Run(async () =>
                {
                    if (!tutState.Contains(TutorialState.AvatarSelection))
                    {
                        int gender = rdoMale.Checked ? 0 : 1;

                        var avatarRes = await session.Client.Player.SetAvatar(new PlayerAvatar()
                        {
                            Backpack = 0,
                            Eyes = 0,
                            Avatar = gender,
                            Hair = 0,
                            Hat = 0,
                            Pants = 0,
                            Shirt = 0,
                            Shoes = 0,
                            Skin = 0
                        });

                        if (avatarRes.Status == SetAvatarResponse.Types.Status.AvatarAlreadySet ||
                            avatarRes.Status == SetAvatarResponse.Types.Status.Success)
                        {
                            encounterTutorialCompleteResponse = session.Client.Misc
                                .MarkTutorialComplete(new RepeatedField<TutorialState>()
                                {
                                    TutorialState.AvatarSelection
                                }).Result;

                            if (encounterTutorialCompleteResponse.Result == EncounterTutorialCompleteResponse.Types.Result.Success)
                            {
                                session.EventDispatcher.Send(new NoticeEvent()
                                {
                                    Message = $"Selected your avatar, now you are {gender}!"
                                });

                                this.Invoke(new Action(() =>
                                {
                                    wizardControl1.NextPage();
                                }), null);

                                return true;
                            }
                        }

                        this.Invoke(new Action(() =>
                        {
                            lblNameError.Text = "Error selecting avatar gender!";
                            lblNameError.Visible = true;
                            wizardControl1.PreviousPage();
                        }));
                    }
                    return true;
                });
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        ///
        /// Handling the window messages
        ///
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == WM_NCHITTEST && (int) message.Result == HTCLIENT)
                message.Result = (IntPtr) HTCAPTION;
        }

        private void wizardPage3_Initialize(object sender, WizardPageInitEventArgs e)
        {
            PokemonId firstPoke = rdoBulbasaur.Checked
                ? PokemonId.Bulbasaur
                : rdoCharmander.Checked
                    ? PokemonId.Charmander
                    : PokemonId.Squirtle;

            Task.Run(() =>
                {
                    if (!tutState.Contains(TutorialState.PokemonCapture))
                    {
                        encounterTutorialCompleteResponse = session.Client.Encounter.EncounterTutorialComplete(firstPoke).Result;

                        if (encounterTutorialCompleteResponse.Result == EncounterTutorialCompleteResponse.Types.Result.Success)
                        {
                            session.EventDispatcher.Send(new NoticeEvent()
                            {
                                Message = $"Caught Tutorial pokemon! it's {firstPoke}!"
                            });

                            this.Invoke(new Action(() =>
                            {
                                wizardControl1.NextPage();
                            }), null);
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                lblNameError.Text = "Error catching tutorial pokemon.";
                                lblNameError.Visible = true;
                                wizardControl1.PreviousPage();
                            }));
                        }
                    }
                });
        }

        private void wizardPage6_Initialize(object sender, WizardPageInitEventArgs e)
        {
            string nickname = txtNick.Text;
            ClaimCodenameResponse res = null;
            
            bool markTutorialComplete = false;
            string errorText = null;
            string warningText = null;
            string infoText = null;
            Task.Run(() =>
                {
                    if (!tutState.Contains(TutorialState.NameSelection))
                    {
                        res = session.Client.Misc.ClaimCodename(nickname).Result;

                        switch (res.Status)
                        {
                            case ClaimCodenameResponse.Types.Status.Unset:
                                errorText = "Unset, somehow";
                                break;
                            case ClaimCodenameResponse.Types.Status.Success:
                                infoText = $"Your name is now: {res.Codename}";
                                markTutorialComplete = true;
                                break;
                            case ClaimCodenameResponse.Types.Status.CodenameNotAvailable:
                                errorText = $"That nickname ({nickname}) isn't available, pick another one!";
                                break;
                            case ClaimCodenameResponse.Types.Status.CodenameNotValid:
                                errorText = $"That nickname ({nickname}) isn't valid, pick another one!";
                                break;
                            case ClaimCodenameResponse.Types.Status.CurrentOwner:
                                warningText = $"You already own that nickname!";
                                markTutorialComplete = true;
                                break;
                            case ClaimCodenameResponse.Types.Status.CodenameChangeNotAllowed:
                                warningText = "You can't change your nickname anymore!";
                                markTutorialComplete = true;
                                break;
                            default:
                                errorText = "Unknown Niantic error while changing nickname.";
                                break;
                        }
                        if (!string.IsNullOrEmpty(infoText))
                        {
                            session.EventDispatcher.Send(new NoticeEvent()
                            {
                                Message = infoText
                            });
                        }
                        else if (!string.IsNullOrEmpty(warningText))
                        {
                            session.EventDispatcher.Send(new WarnEvent()
                            {
                                Message = warningText
                            });
                        }
                        else if (!string.IsNullOrEmpty(errorText))
                        {
                            session.EventDispatcher.Send(new ErrorEvent()
                            {
                                Message = errorText
                            });
                        }

                        if (markTutorialComplete)
                        {
                            encounterTutorialCompleteResponse = session.Client.Misc.MarkTutorialComplete(new RepeatedField<TutorialState>()
                            {
                                TutorialState.NameSelection
                            }).Result;

                            if (encounterTutorialCompleteResponse.Result == EncounterTutorialCompleteResponse.Types.Result.Success)
                            {
                                if (!tutState.Contains(TutorialState.FirstTimeExperienceComplete))
                                {
                                    encounterTutorialCompleteResponse = session.Client.Misc.MarkTutorialComplete(new RepeatedField<TutorialState>()
                                    {
                                        TutorialState.FirstTimeExperienceComplete
                                    }).Result;

                                    if (encounterTutorialCompleteResponse.Result == EncounterTutorialCompleteResponse.Types.Result.Success)
                                    {
                                        session.EventDispatcher.Send(new NoticeEvent()
                                        {
                                            Message = "First time experience complete, looks like i just spinned an virtual pokestop :P"
                                        });

                                        this.Invoke(new Action(() =>
                                        {
                                            this.DialogResult = DialogResult.OK;
                                            this.Close();
                                        }));

                                        return;
                                    }
                                }
                            }
                        }

                        this.Invoke(new Action(() =>
                        {
                            lblNameError.Text = errorText;
                            lblNameError.Visible = true;
                            wizardControl1.PreviousPage();
                        }));
                    }
                });
        }

        private void wizardPage1_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (tutState.Contains(TutorialState.AvatarSelection))
            {
                wizardControl1.NextPage(Step2);
            }
        }

        private void wizardPage2_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (tutState.Contains(TutorialState.PokemonCapture))
            {
                wizardControl1.NextPage(Step3);
            }
        }

        private void wizardPage5_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (tutState.Contains(TutorialState.NameSelection))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}