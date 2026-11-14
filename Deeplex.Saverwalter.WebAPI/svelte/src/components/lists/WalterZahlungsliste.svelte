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
    Liste von Zahlungen unter einer Überschrift, z. B. Guthaben oder rückgängig gemachte
    Zahlungen — in derselben Form wie aufgeklappte Tabellenzeilen (WalterEintraege). Mit
    `bearbeitbar` je Zahlung „Rückgängig machen“, mit `rueckgaengigGemacht` stattdessen
    der Tag „Rückgängig gemacht“.
-->
<script lang="ts">
    import WalterEintraege from '../elements/WalterEintraege.svelte';
    import WalterRueckgaengig from '../details/WalterRueckgaengig.svelte';
    import { ZAHLUNGS_STATUS, type WalterZahlungsEintrag } from '$walter/lib';
    import { createEventDispatcher } from 'svelte';

    export let titel: string;
    export let eintraege: WalterZahlungsEintrag[];
    export let bearbeitbar = false;
    export let rueckgaengigGemacht = false;
    export let fetchImpl: typeof fetch = fetch;

    /** `erledigt`: eine Zahlung wurde rückgängig gemacht. */
    const dispatch = createEventDispatcher<{ erledigt: void }>();

    $: angezeigt = rueckgaengigGemacht
        ? eintraege.map((e) => ({
              ...e,
              tag: {
                  text: ZAHLUNGS_STATUS.rueckgaengig.text,
                  tag: ZAHLUNGS_STATUS.rueckgaengig.tag
              }
          }))
        : eintraege;
</script>

{#if eintraege.length > 0}
    <h5 class="titel">{titel}</h5>
    <WalterEintraege eintraege={angezeigt}>
        <svelte:fragment slot="aktion" let:eintrag>
            {#if bearbeitbar && !rueckgaengigGemacht}
                <WalterRueckgaengig
                    {fetchImpl}
                    transaktionId={eintrag.transaktionId}
                    buchungssatzId={eintrag.buchungssatzId}
                    on:erledigt={() => dispatch('erledigt')}
                />
            {/if}
        </svelte:fragment>
    </WalterEintraege>
{/if}

<style>
    .titel {
        margin: 1.5rem 0 0.5rem 0;
    }
</style>
