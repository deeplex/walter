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
    import {
        WalterGrid,
        WalterHeader,
        WalterTransaktion
    } from '$walter/components';
    import WalterBuchungsvorschau from '$walter/components/details/WalterBuchungsvorschau.svelte';
    import {
        HeaderGlobalAction,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import { Save } from 'carbon-icons-svelte';
    import {
        emptyTransaktionsInput,
        type TransaktionsInput,
        WalterToastContent
    } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';
    import { walter_goto } from '$walter/services/utils';
    import { addToast, changeTracker } from '$walter/store';
    import type { PageData } from './$types';

    export let data: PageData;

    let buchung: TransaktionsInput = emptyTransaktionsInput();
    let buchungValid = false;
    let vorschauGueltig = false;
    // Die Vorschau zeigt die Zahlung; reine NK-Anteile (ohne Betrag) haben keine.
    $: mitZahlung = buchung.betrag > 0;
    $: speicherbar = buchungValid && (!mitZahlung || vorschauGueltig);

    const SaveToast = new WalterToastContent(
        'Buchung gespeichert',
        'Buchung fehlgeschlagen',
        (a: unknown) => a as string,
        (a: unknown) =>
            `Buchung fehlgeschlagen: ${
                typeof a === 'object' && a !== null && 'title' in a
                    ? (a as { title: string }).title
                    : JSON.stringify(a)
            }`
    );

    const InvalidToast = new WalterToastContent(
        '',
        'Betrag nicht vollständig verteilt',
        () => '',
        () =>
            'Der Transaktionsbetrag muss vollständig auf Positionen verteilt sein.'
    );

    async function save() {
        if (!speicherbar) {
            addToast(InvalidToast, false, null);
            return;
        }
        const response = await walter_post(
            '/api/transaktionen/buchen',
            buchung
        );
        const parsed = await response.json();
        addToast(SaveToast, response.status === 200, parsed);
        if (parsed.id) {
            changeTracker.set(0);
            walter_goto(`/transaktionen/${parsed.id}`);
        }
    }
</script>

<WalterHeader title="Neue Buchung">
    <HeaderUtilities>
        <HeaderGlobalAction
            on:click={save}
            icon={Save}
            disabled={!speicherbar}
        />
    </HeaderUtilities>
</WalterHeader>

<WalterGrid>
    <WalterTransaktion
        fetchImpl={data.fetchImpl}
        bind:buchung
        bind:isValid={buchungValid}
    />
    {#if mitZahlung}
        <WalterBuchungsvorschau
            input={buchungValid ? buchung : undefined}
            bind:gueltig={vorschauGueltig}
        />
    {/if}
</WalterGrid>
