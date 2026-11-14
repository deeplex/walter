<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<!--
    Rahmen einer Liste: inline oder als aufklappbarer Abschnitt „Titel (Anzahl)“. Im Slot
    `titel` steht Zusätzliches im Abschnittskopf, z. B. ein Tag „120,00 € offen“; im Slot
    `aktionen` Knöpfe wie „+ Rechnung“, rechts oben im aufgeklappten Teil (im Kopf würden
    sie den Abschnitt auf- und zuklappen). Mit `id` ist der Abschnitt ein Sprungziel; zeigt
    die Adresse darauf (`#id`), ist er offen.
-->
<script lang="ts">
    import { Accordion, AccordionItem, Tile } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let layout: 'inline' | 'accordion' = 'inline';
    export let accordionTitle: string = '';
    export let count = 0;
    export let initialOpen = false;
    export let id: string | undefined = undefined;

    let open = initialOpen;
    onMount(() => {
        const zeigtHierher = () => {
            if (id && location.hash === `#${id}`) open = true;
        };
        zeigtHierher();
        window.addEventListener('hashchange', zeigtHierher);
        return () => window.removeEventListener('hashchange', zeigtHierher);
    });
</script>

{#if layout === 'accordion'}
    <Accordion {id}>
        <AccordionItem bind:open>
            <svelte:fragment slot="title">
                <span class="titel">
                    {`${accordionTitle} (${count})`}
                    <slot name="titel" />
                </span>
            </svelte:fragment>
            <Tile style="overflow: auto">
                {#if $$slots.aktionen}
                    <div class="aktionen"><slot name="aktionen" /></div>
                {/if}
                <slot />
            </Tile>
        </AccordionItem>
    </Accordion>
{:else}
    {#if $$slots.aktionen}
        <div class="aktionen"><slot name="aktionen" /></div>
    {/if}
    <slot />
{/if}

<style>
    .aktionen {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
        margin-bottom: 0.5rem;
    }
    .titel {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
    }
</style>
