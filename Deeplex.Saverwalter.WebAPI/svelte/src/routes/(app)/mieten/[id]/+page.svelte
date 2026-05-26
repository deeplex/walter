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
    import { WalterGrid, WalterHeader } from '$walter/components';
    import { WalterLinkTile, WalterLinks, WalterNumberInput } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;

    const entry = data.entry;
    const title = entry.vertrag?.text ?? 'Mietzahlung';
    const monat = entry.betreffenderMonat
        ? new Date(entry.betreffenderMonat).toLocaleDateString('de-DE', {
              month: 'long',
              year: 'numeric'
          })
        : '';
    const datum = entry.buchungsdatum
        ? new Date(entry.buchungsdatum).toLocaleDateString('de-DE')
        : '';
</script>

<WalterHeader {title} />

<WalterGrid>
    <Row>
        <WalterNumberInput readonly label="Betrag" value={entry.betrag} />
    </Row>
    <Row>
        <p style="margin: 1rem 1rem 0"><strong>Betreffender Monat:</strong> {monat}</p>
        <p style="margin: 1rem 1rem 0"><strong>Zahlungsdatum:</strong> {datum}</p>
    </Row>
    {#if entry.vertrag}
        <WalterLinks>
            <WalterLinkTile
                fileref=""
                name={`Vertrag: ${entry.vertrag.text}`}
                href={`/vertraege/${entry.vertrag.id}`}
            />
        </WalterLinks>
    {/if}
</WalterGrid>
