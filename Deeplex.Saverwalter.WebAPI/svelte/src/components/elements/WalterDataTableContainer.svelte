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

<script lang="ts">
    import { Accordion, AccordionItem, Tile } from 'carbon-components-svelte';

    export let layout: 'inline' | 'accordion' = 'inline';
    export let accordionTitle: string = '';
    export let count = 0;
    export let initialOpen = false;

    let open = initialOpen;
</script>

{#if layout === 'accordion'}
    <Accordion>
        <AccordionItem title={`${accordionTitle} (${count})`} bind:open>
            <Tile style="overflow: auto">
                <slot />
            </Tile>
        </AccordionItem>
    </Accordion>
{:else}
    <slot />
{/if}
