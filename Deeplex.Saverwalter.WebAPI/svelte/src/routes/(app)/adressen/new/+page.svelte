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
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderNew,
        WalterTextArea
    } from '$walter/components';
    import { WalterAdresseEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    const title = 'Neue Adresse';

    const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew apiURL={WalterAdresseEntry.ApiURL} {entry} {title} />

<WalterGrid>
    <WalterAdresse {entry} />
    <Row>
        <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
    </Row>
</WalterGrid>
