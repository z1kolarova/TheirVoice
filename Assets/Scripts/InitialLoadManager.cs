using Assets.Classes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoadManager : MonoBehaviour
{
    [SerializeField] private bool buildServer = false;

    void Start()
    {
        //var list = new List<PromptLabel>() { 
        //    new PromptLabel ("angry_religious_carnist", EndConvoAbility.Never, ArgumentationTag.HumanSuperiority | ArgumentationTag.Religion),
        //    new PromptLabel ("athlete_eats_fish_but_vegetarian_label", EndConvoAbility.Sometimes, ArgumentationTag.Vegetarian),
        //    new PromptLabel ("baking_grandparent", EndConvoAbility.Sometimes, ArgumentationTag.Nutrition | ArgumentationTag.HumaneFarming),
        //    new PromptLabel ("convenient_vegetarian", EndConvoAbility.Sometimes, ArgumentationTag.ItsHard | ArgumentationTag.Vegetarian),
        //    new PromptLabel ("endless_stream_of_questions", EndConvoAbility.Sometimes, ArgumentationTag.None),
        //    new PromptLabel ("guiltfree_meat_is_healthy_and_it_is_what_it_is", EndConvoAbility.Always, ArgumentationTag.SystemsFault),
        //    new PromptLabel ("meateater_with_vegetarian_girlfriend", EndConvoAbility.Sometimes, ArgumentationTag.Lions | ArgumentationTag.HumanSuperiority),
        //    new PromptLabel ("sad_single_parent", EndConvoAbility.Sometimes, ArgumentationTag.ItsHard),
        //    new PromptLabel ("teen_troll_with_pet_bunny", EndConvoAbility.Never, ArgumentationTag.Troll),
        //    new PromptLabel ("troll_too_cool_for_this", EndConvoAbility.Never, ArgumentationTag.Troll),
        //    new PromptLabel ("vegetarian_is_enough_loves_cheese", EndConvoAbility.Sometimes, ArgumentationTag.Vegetarian | ArgumentationTag.Taste),
        //    new PromptLabel ("wellfarist", EndConvoAbility.Sometimes, ArgumentationTag.HumaneFarming),
        //};

        //ConvoUtilsGPT.SerializePromptBank(list);

        if (buildServer)
        {
            SceneManager.LoadScene(sceneName: "Scenes/Server");
        }
        else
        {
            SceneManager.LoadScene(sceneName: "Scenes/MainMenu");
        }
    }
}
