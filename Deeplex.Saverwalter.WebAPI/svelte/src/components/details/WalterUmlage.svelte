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
        WalterComboBox,
        WalterDatePicker,
        WalterMultiSelectWohnung,
        WalterTextArea,
        WalterMultiSelectZaehler,
        WalterComboBoxUmlagetyp
    } from '$walter/components';

    import WalterUmlageHKVO from './WalterUmlageHKVO.svelte';

    import { Row } from 'carbon-components-svelte';
    import type { WalterUmlageEntry } from '$walter/lib';
    import { WalterToastContent, WalterUmlageVersionEntry } from '$walter/lib';
    import type { WalterSelectionEntry } from '$walter/lib';
    import { walter_selection, walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';

    export let entry: Partial<WalterUmlageEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let blockSave = false;

    $: readonly = entry?.permissions?.update === false;
    $: isNew = !entry.id;

    $: newVersion = (entry.versionen?.[0] ??
        {}) as Partial<WalterUmlageVersionEntry>;

    $: sortedVersionen = [...(entry.versionen ?? [])].sort((a, b) =>
        a.beginn.localeCompare(b.beginn)
    );
    $: latestVersion = sortedVersionen.at(-1);

    // In new mode: bind directly to newVersion.schluessel
    // In edit mode: use a copy for change detection
    let editSchluessel: WalterSelectionEntry | undefined;
    let initialized = false;
    let seitWann: string | undefined;

    $: if (!initialized && !isNew && latestVersion?.schluessel) {
        editSchluessel = { ...latestVersion.schluessel };
        initialized = true;
    }

    $: hasVersionChanges =
        !isNew &&
        initialized &&
        editSchluessel?.id !== latestVersion?.schluessel?.id;

    $: blockSave = (hasVersionChanges && !seitWann) || hkvoBlockSave;

    let hkvoBlockSave = false;

    // Current displayed schluessel for HKVO check (works for both new and edit)
    $: currentSchluessel = isNew ? newVersion.schluessel : editSchluessel;

    export let commitVersionIfPending: () => Promise<void> = async () => {};
    $: commitVersionIfPending = async () => {
        if (hasVersionChanges && seitWann) await createNewVersion();
    };

    async function createNewVersion() {
        if (!entry.id || !seitWann) return;

        const toast = new WalterToastContent(
            'Version gespeichert',
            'Fehler beim Speichern',
            () => 'Neue Umlageschlüssel-Version erfolgreich erstellt.',
            (e: unknown) => `Konnte Version nicht speichern: ${e}`
        );

        const response = await walter_post(WalterUmlageVersionEntry.ApiURL, {
            umlage: { id: entry.id, text: '' },
            beginn: seitWann,
            schluessel: editSchluessel
        });
        const parsed = await response.json();
        addToast(toast, response.status === 200, parsed);

        if (response.status === 200) {
            entry.versionen = [
                ...(entry.versionen ?? []),
                parsed as WalterUmlageVersionEntry
            ];
            initialized = false;
            seitWann = undefined;
        }
    }

    const umlageschluessel = walter_selection.umlageschluessel(fetchImpl);
</script>

<Row>
    <WalterComboBoxUmlagetyp
        {readonly}
        {fetchImpl}
        required
        bind:value={entry.typ}
        title="Betriebskostentyp der Umlage"
    />
    {#if isNew}
        <WalterComboBox
            required
            {readonly}
            entries={umlageschluessel}
            bind:value={newVersion.schluessel}
            titleText="Umlageschlüssel"
        />
        <WalterDatePicker
            required
            disabled={readonly}
            bind:value={newVersion.beginn}
            labelText="Beginn"
        />
    {:else}
        <WalterComboBox
            required
            {readonly}
            entries={umlageschluessel}
            bind:value={editSchluessel}
            titleText="Umlageschlüssel"
        />
    {/if}
</Row>
<Row>
    <WalterMultiSelectWohnung
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedWohnungen}
        titleText="Wohnungen"
    />
</Row>
<!-- If schluessel is nach Verbrauch (id=3) -->
{#if currentSchluessel?.id === 3}
    <Row>
        <WalterMultiSelectZaehler
            {fetchImpl}
            {readonly}
            bind:value={entry.selectedZaehler}
            titleText="Zähler"
        />
    </Row>
{/if}

<Row>
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
    />
</Row>
<Row>
    <WalterTextArea
        {readonly}
        labelText="Beschreibung"
        bind:value={entry.beschreibung}
    />
</Row>

{#if hasVersionChanges}
    <Row>
        <WalterDatePicker
            required
            bind:value={seitWann}
            labelText="Zeitpunkt der Änderung"
        />
    </Row>
{/if}

<WalterUmlageHKVO
    bind:entry
    {fetchImpl}
    bind:selectedWohnungen={entry.selectedWohnungen}
    schluessel={currentSchluessel}
    bind:blockSave={hkvoBlockSave}
/>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
