<?php

namespace App\Form\Type;

use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\Form\Extension\Core\Type\ColorType;
use Symfony\Component\Form\CallbackTransformer;

// custom version of ColorType which returns an integer
class IntegerColorType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options)
    {
        $builder->addModelTransformer(new CallbackTransformer(
            function (int $color): string {
                return "#" . dechex($color);
            },
            function (string $color): int {
                if (substr($color, 0, 1) == "#")
                {
                    $color = substr($color, 1);
                }

                return hexdec($color);
            }
        ));
    }

    public function getParent(): string
    {
        return ColorType::class;
    }
}