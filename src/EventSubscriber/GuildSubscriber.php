<?php

namespace App\EventSubscriber;

use Symfony\Component\HttpKernel\Exception\NotFoundHttpException;
use Symfony\Component\EventDispatcher\EventSubscriberInterface;
use Symfony\Component\HttpKernel\Event\ControllerEvent;
use Symfony\Component\HttpKernel\KernelEvents;
use App\Controller\GuildController;
use App\Entity\Guild;
use Doctrine\Persistence\ManagerRegistry;

class GuildSubscriber implements EventSubscriberInterface
{
    private ManagerRegistry $doctrine;

    public function __construct(ManagerRegistry $doctrine)
    {
        $this->doctrine = $doctrine;
    }

    public function onKernelController(ControllerEvent $event)
    {
        $controller = $event->getController();

        // when a controller class defines multiple action methods, the controller
        // is returned as [$controllerInstance, 'methodName']
        if (is_array($controller))
        {
            $controller = $controller[0];
        }

        if ($controller instanceof GuildController)
        {
            $guilds = $this->doctrine->getManager()->getRepository(Guild::class);
            $id = $event->getRequest()->attributes->get("id");

            // check if guild exists
            if (!$guilds->find($id))
            {
                throw new NotFoundHttpException("Guild not found");
            }
        }
    }

    public static function getSubscribedEvents()
    {
        return [
            KernelEvents::CONTROLLER => "onKernelController"
        ];
    }
}