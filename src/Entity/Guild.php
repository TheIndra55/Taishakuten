<?php

namespace App\Entity;

use Doctrine\ORM\Mapping\Entity;
use Doctrine\ORM\Mapping\Table;
use Doctrine\ORM\Mapping\Id;
use Doctrine\ORM\Mapping\Column;

#[Entity]
#[Table(name: "Guilds")]
class Guild
{
    #[Id]
    #[Column(name: "Id", type: "bigint", nullable: false)]
    public $id;
}