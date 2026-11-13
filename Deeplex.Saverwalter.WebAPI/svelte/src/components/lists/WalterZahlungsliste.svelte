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
    Schlichte Liste von Zahlungen unter einer Überschrift, z. B. Guthaben oder rückgängig
    gemachte Zahlungen. Mit `bearbeitbar` je Zahlung „Rückgängig machen“, mit
    `rueckgaengigGemacht` stattdessen der Tag „Rückgängig gemacht“.
-->
<script lang="ts">
    import { Tag } from 'carbon-components-svelte';
    import WalterRueckgaengig from '../details/WalterRueckgaengig.svelte';
    import type { WalterZahlungsEintrag } from '$walter/lib';
    import { createEventDispatcher } from 'svelte';

    export let titel: string;
    export let eintraege: WalterZahlungsEintrag[];
    export let bearbeitbar = false;
    export let rueckgaengigGemacht = false;
    export let fetchImpl: typeof fetch = fetch;

    /** `erledigt`: eine Zahlung wurde rückgängig gemacht. */
    const dispatch = createEventDispatcher<{ erledigt: void }>();

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');
</script>

{#if eintraege.length > 0}
    <h5 style="margin-top: 1rem">{titel}</h5>
    <ul class="liste">
        {#each eintraege as z (z.id)}
            <li>
                <span>{datumText(z.datum)}</span>
                <span>{eur(z.betrag)}</span>
                <span class="text">{z.text}</span>
                {#if rueckgaengigGemacht}
                    <Tag size="sm" type="gray">Rückgängig gemacht</Tag>
                {/if}
                <a href={z.href}>Details →</a>
                {#if bearbeitbar && !rueckgaengigGemacht}
                    <WalterRueckgaengig
                        {fetchImpl}
                        transaktionId={z.transaktionId}
                        buchungssatzId={z.buchungssatzId}
                        on:erledigt={() => dispatch('erledigt')}
                    />
                {/if}
            </li>
        {/each}
    </ul>
{/if}

<style>
    .liste {
        list-style: none;
        margin: 0.5rem 0 0 0;
        padding: 0;
    }
    .liste li {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        align-items: center;
        padding: 0.25rem 0;
    }
    .text {
        flex: 1;
    }
</style>
