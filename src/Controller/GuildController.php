<?php

namespace App\Controller;

use Symfony\Component\Routing\Annotation\Route;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\Form\Extension\Core\Type\SubmitType;
use Symfony\Component\Form\Extension\Core\Type\CheckboxType;
use Doctrine\Persistence\ManagerRegistry;
use App\Entity\Welcome;
use App\Form\Type\IntegerColorType;

class GuildController extends AbstractController
{
    private ManagerRegistry $doctrine;

    public function __construct(ManagerRegistry $doctrine)
    {
        $this->doctrine = $doctrine;
    }

    #[Route("/guilds/{id}")]
    public function show(int $id): Response
    {
        return $this->render("guild/index.html.twig", ["guild_id" => $id]);
    }

    #[Route("/guilds/{id}/welcome", methods: ["GET", "POST"])]
    public function welcome(int $id, Request $request): Response
    {
        $em = $this->doctrine->getManager();
        $welcomes = $em->getRepository(Welcome::class);

        // fetch the current welcome
        $welcome = $welcomes->find($id);

        $form = $this->createFormBuilder($welcome)
            ->add("title", TextType::class)
            ->add("body", TextType::class)
            ->add("color", IntegerColorType::class)
            ->add("mention", CheckboxType::class, ["required" => false, "label" => "Should mention"])
            ->add("save", SubmitType::class, ["label" => "Save"])
            ->getForm();

        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid())
        {
            // update database
            $em->flush();
        }

        return $this->renderForm("guild/welcome.html.twig", ["form" => $form, "embed_color" => dechex($welcome->color), "welcome" => $welcome]);
    }
}