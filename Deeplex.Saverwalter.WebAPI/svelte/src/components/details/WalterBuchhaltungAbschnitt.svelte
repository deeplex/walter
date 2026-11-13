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
    Eingeklappter Abschnitt „Buchhaltung“ ganz unten auf jeder Detailseite: die
    Konten des Objekts, wie sie in der Buchhaltung geführt werden. Gleicher Titel
    und gleiche Position auf jeder Seite; weitere Buchhaltungsinhalte über den Slot.
-->
<script lang="ts">
    import { Accordion, AccordionItem } from 'carbon-components-svelte';
    import WalterBuchungskonten from '../lists/WalterBuchungskonten.svelte';
    import type { WalterBuchungskontoEntry } from '$walter/lib';

    export let konten: Partial<WalterBuchungskontoEntry>[] = [];
</script>

{#if konten.length > 0 || $$slots.default}
    <Accordion size="xl" style="padding-bottom: 1em">
        <AccordionItem title="Buchhaltung">
            {#if konten.length > 0}
                <WalterBuchungskonten rows={konten} />
            {/if}
            <slot />
        </AccordionItem>
    </Accordion>
{/if}
