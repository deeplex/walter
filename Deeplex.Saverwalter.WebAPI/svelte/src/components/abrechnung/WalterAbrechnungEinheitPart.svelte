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
    import { convertDateGerman, convertPercent } from '$walter/services/utils';
    import {
        StructuredListCell,
        StructuredListRow
    } from 'carbon-components-svelte';

    export let beginn: Date;
    export let ende: Date;
    export let value: string | number | undefined;
    export let gesamt: string | number | undefined;
    export let anteil: number;
    export let tage: number;
</script>

<StructuredListRow>
    <StructuredListCell>
        {value} / {gesamt}
    </StructuredListCell>
    <StructuredListCell>
        {convertDateGerman(new Date(beginn))} - {convertDateGerman(
            new Date(ende)
        )}
    </StructuredListCell>
    <StructuredListCell>{tage}</StructuredListCell>
    <StructuredListCell>{convertPercent(anteil)}</StructuredListCell>
</StructuredListRow>
