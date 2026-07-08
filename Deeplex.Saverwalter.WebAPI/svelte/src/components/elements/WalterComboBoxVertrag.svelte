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
    import type { WalterSelectionEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { WalterComboBox } from '..';

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;

    let entries = walter_selection.vertraege(fetchImpl);
</script>

<div style="flex: 1 1 auto !important">
    <WalterComboBox
        {required}
        {readonly}
        bind:value
        titleText="Wohnungsvertrag (optional)"
        {entries}
    />
</div>
