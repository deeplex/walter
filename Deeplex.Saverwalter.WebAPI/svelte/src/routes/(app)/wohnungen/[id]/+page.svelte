<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import type { PageData } from './$types';
    import {
        WalterBuchungskonten,
        WalterGrid,
        WalterWohnungen,
        WalterVertraege,
        WalterZaehlerList,
        WalterUmlagen,
        WalterHeaderDetail,
        WalterWohnung,
        WalterWohnungEigentuemer,
        WalterWohnungVersionen,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import {
        WalterFileWrapper,
        WalterWohnungEigentuemerEntry,
        WalterVertragVersionEntry,
        type WalterUmlageEntry,
        type WalterVertragEntry,
        type WalterWohnungVersionEntry,
        type WalterZaehlerEntry,
        validateWohnung
    } from '$walter/lib';
    import { changeTracker } from '$walter/store';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    const zaehlerEntry: Partial<WalterZaehlerEntry> = {
        wohnung: {
            id: `${data.entry.id}`,
            text: `${data.entry.adresse?.anschrift} - ${data.entry.bezeichnung}`
        },
        adresse: data.entry.adresse ? { ...data.entry.adresse } : undefined,
        permissions: data.entry.permissions
    };
    const umlageEntry: Partial<WalterUmlageEntry> = {
        selectedWohnungen: [
            {
                id: '' + data.entry.id,
                text:
                    data.entry.adresse?.anschrift +
                    ' - ' +
                    data.entry.bezeichnung
            }
        ],
        permissions: data.entry.permissions
    };
    let title = data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung;
    $: {
        title = data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);

    const wohnungversionEntry: Partial<WalterWohnungVersionEntry> = {
        wohnung: {
            id: '' + data.entry.id,
            text: data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung
        },
        permissions: data.entry.permissions
    };

    const vertragEntry: Partial<WalterVertragEntry> = {
        wohnung: {
            id: '' + data.entry.id,
            text: data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung
        },
        versionen: [{} as WalterVertragVersionEntry],
        permissions: data.entry.permissions
    };

    const eigentuemerEntry: Partial<WalterWohnungEigentuemerEntry> = {
        wohnung: {
            id: '' + data.entry.id,
            text: data.entry.adresse?.anschrift + ' - ' + data.entry.bezeichnung
        },
        permissions: data.entry.permissions
    };
    const eigentuemerRows = data.entry
        .eigentuemer as unknown as WalterWohnungEigentuemerEntry[];

    let blockSave = false;
    let commitVersionIfPending: () => Promise<void>;
    $: submitDisabled =
        $changeTracker === 0 || !validateWohnung(data.entry) || blockSave;
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
    disabled={submitDisabled}
    beforeSave={commitVersionIfPending}
/>

<WalterGrid>
    <WalterWohnung
        fetchImpl={data.fetchImpl}
        bind:entry={data.entry}
        bind:blockSave
        bind:commitVersionIfPending
    />

    <WalterLinks>
        <WalterWohnungVersionen
            entry={wohnungversionEntry}
            title="Versionen"
            rows={data.entry.versionen}
        />
        <WalterWohnungEigentuemer
            fetchImpl={data.fetchImpl}
            entry={eigentuemerEntry}
            title="Eigentümer"
            rows={eigentuemerRows}
        />
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen an der selben Adresse"
            rows={data.entry.haus || []}
        />
        <WalterZaehlerList
            fetchImpl={data.fetchImpl}
            entry={zaehlerEntry}
            title="Zähler"
            rows={data.entry.zaehler}
        />
        <WalterVertraege
            fetchImpl={data.fetchImpl}
            title="Verträge"
            entry={vertragEntry}
            rows={data.entry.vertraege}
        />
        <WalterUmlagen
            fetchImpl={data.fetchImpl}
            entry={umlageEntry}
            title="Umlagen"
            rows={data.entry.umlagen}
        />
        <WalterBuchungskonten title="Konten" rows={data.entry.konten} />

        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.adresse(`${data.entry.adresse.id}`)}
            name={`Adresse: ${data.entry.adresse.anschrift}`}
            href={`/adressen/${data.entry.adresse.id}`}
        />

        <!-- TODO besitzer id is guid -->
    </WalterLinks>
</WalterGrid>
