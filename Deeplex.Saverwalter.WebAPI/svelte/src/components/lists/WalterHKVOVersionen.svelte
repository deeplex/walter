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
    import { WalterDataTable } from '$walter/components';
    import type { WalterHKVOEntry } from '$walter/lib';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'hkvO_P7', value: '§7 (%)' },
        { key: 'hkvO_P8', value: '§8 (%)' },
        { key: 'hkvO_P9', value: '§9' },
        { key: 'strompauschale', value: 'Strompauschale (%)' },
        { key: 'stromrechnung', value: 'Stromrechnung' }
    ];

    export let rows: WalterHKVOEntry[] | undefined = undefined;
    export let title: string | undefined = undefined;

    $: displayRows = (rows ?? []).map((hkvo) => ({
        id: hkvo.id,
        beginn: hkvo.beginn,
        hkvO_P7: hkvo.hkvO_P7,
        hkvO_P8: hkvo.hkvO_P8,
        hkvO_P9: hkvo.hkvO_P9?.text,
        strompauschale: hkvo.strompauschale,
        stromrechnung: hkvo.stromrechnung?.text,
        permissions: hkvo.permissions
    }));
</script>

<WalterDataTable
    readonly
    layout="accordion"
    accordionTitle={title}
    {headers}
    rows={displayRows}
/>
