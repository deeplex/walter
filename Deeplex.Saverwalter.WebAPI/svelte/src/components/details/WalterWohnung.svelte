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
        WalterAdresse,
        WalterComboBoxKontakt,
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterWohnungEntry } from '$walter/lib';
    import { WalterToastContent, WalterWohnungVersionEntry } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';

    export let entry: Partial<WalterWohnungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let blockSave = false;

    $: readonly = entry?.permissions?.update === false;
    $: isNew = !entry.id;

    $: newVersion = (entry.versionen?.[0] ??
        {}) as Partial<WalterWohnungVersionEntry>;

    $: sortedVersionen = [...(entry.versionen ?? [])].sort((a, b) =>
        a.beginn.localeCompare(b.beginn)
    );
    $: latestVersion = sortedVersionen.at(-1);

    let displayVersion: Partial<WalterWohnungVersionEntry> = {};
    let initialized = false;
    let seitWann: string | undefined;

    $: if (!initialized) {
        const source = isNew ? newVersion : latestVersion;
        if (source) {
            displayVersion = isNew
                ? newVersion
                : {
                      wohnflaeche: source.wohnflaeche,
                      nutzflaeche: source.nutzflaeche,
                      miteigentumsanteile: source.miteigentumsanteile,
                      einheiten: source.einheiten
                  };
            initialized = true;
        }
    }

    $: hasVersionChanges =
        !isNew &&
        initialized &&
        (displayVersion.wohnflaeche !== latestVersion?.wohnflaeche ||
            displayVersion.nutzflaeche !== latestVersion?.nutzflaeche ||
            displayVersion.miteigentumsanteile !==
                latestVersion?.miteigentumsanteile ||
            displayVersion.einheiten !== latestVersion?.einheiten);

    $: blockSave = hasVersionChanges && !seitWann;

    export let commitVersionIfPending: () => Promise<void> = async () => {};
    $: commitVersionIfPending = async () => {
        if (hasVersionChanges && seitWann) await createNewVersion();
    };

    async function createNewVersion() {
        if (!entry.id || !seitWann) return;

        const toast = new WalterToastContent(
            'Version gespeichert',
            'Fehler beim Speichern',
            () => 'Neue Wohnungsversion erfolgreich erstellt.',
            (e: unknown) => `Konnte Version nicht speichern: ${e}`
        );

        const response = await walter_post(WalterWohnungVersionEntry.ApiURL, {
            wohnung: { id: entry.id, text: '' },
            beginn: seitWann,
            wohnflaeche: displayVersion.wohnflaeche,
            nutzflaeche: displayVersion.nutzflaeche,
            miteigentumsanteile: displayVersion.miteigentumsanteile,
            einheiten: displayVersion.einheiten
        });
        const parsed = await response.json();
        addToast(toast, response.status === 200, parsed);

        if (response.status === 200) {
            entry.versionen = [
                ...(entry.versionen ?? []),
                parsed as WalterWohnungVersionEntry
            ];
            initialized = false;
            seitWann = undefined;
        }
    }
</script>

<Row>
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.besitzer}
        title="Besitzer"
    />
</Row>
<WalterAdresse required {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    {#if isNew}
        <WalterDatePicker
            required
            disabled={readonly}
            bind:value={newVersion.beginn}
            labelText="Beginn"
        />
    {/if}
    <WalterNumberInput
        required
        {readonly}
        bind:value={displayVersion.wohnflaeche}
        label="Wohnfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={displayVersion.nutzflaeche}
        label="Nutzfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={displayVersion.miteigentumsanteile}
        label="Miteigentumsanteile"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={displayVersion.einheiten}
        label="Einheiten"
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
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
