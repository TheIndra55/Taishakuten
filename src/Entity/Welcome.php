<?php

namespace App\Entity;

use Doctrine\ORM\Mapping\Entity;
use Doctrine\ORM\Mapping\Table;
use Doctrine\ORM\Mapping\Id;
use Doctrine\ORM\Mapping\Column;

use Symfony\Component\Validator\Constraints as Assert;

#[Entity]
#[Table(name: "Welcomes")]
class Welcome
{
    #[Id]
    #[Column(name: "Guild", type: "bigint", nullable: false)]
    public $guild;

    #[Column(name: "Channel", type: "bigint", nullable: false)]
    public $channel;

    #[Column(name: "Title", type: "string", length: 256)]
    #[Assert\NotBlank]
    #[Assert\Length(max: 256)]
    public $title;
    
    #[Column(name: "Body", type: "string", length: 4096)]
    #[Assert\NotBlank]
    #[Assert\Length(max: 4096)]
    public $body;

    #[Column(name: "Color", type: "integer", nullable: false)]
    public $color;

    #[Column(name: "Mention", type: "boolean", nullable: false)]
    public $mention;
}