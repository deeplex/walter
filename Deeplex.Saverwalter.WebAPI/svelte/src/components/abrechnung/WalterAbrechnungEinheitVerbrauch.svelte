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
        convertDateGerman,
        convertFixed2,
        convertPercent
    } from '$walter/services/utils';
    import type { WalterVerbrauchAnteil } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        StructuredListCell,
        StructuredListRow
    } from 'carbon-components-svelte';
    import { WalterLink } from '..';

    export let verbrauch: WalterVerbrauchAnteil;
    export let beginn: Date;
    export let ende: Date;
</script>

{#each Object.entries(verbrauch.dieseZaehler) as [unit, verbrauchUnit]}
    {#each verbrauchUnit as v}
        <StructuredListRow>
            <StructuredListCell>
                {convertFixed2(v.delta)}
                {v.zaehler.filter} / {convertFixed2(
                    verbrauch.alleVerbrauch[unit]
                )}
                {v.zaehler.filter}
            </StructuredListCell>
            <StructuredListCell></StructuredListCell>
            <StructuredListCell>
                <WalterLink href={`zaehler/${v.zaehler.id}`}>
                    {v.zaehler.text}
                </WalterLink>
            </StructuredListCell>
        </StructuredListRow>
    {/each}
    <StructuredListRow>
        <StructuredListCell head>
            {convertFixed2(verbrauch.dieseVerbrauch[unit])}
            {verbrauchUnit[0]?.zaehler?.filter || '?'} / {convertFixed2(
                verbrauch.alleVerbrauch[unit]
            )}
            {verbrauchUnit[0]?.zaehler?.filter || '?'}
        </StructuredListCell>
        <StructuredListCell>
            {convertDateGerman(new Date(beginn))} - {convertDateGerman(
                new Date(ende)
            )}
        </StructuredListCell>
        <StructuredListCell head>
            <WalterLink href={`umlagen/${verbrauch.umlage.id}`}>
                {verbrauch.umlage.text}
            </WalterLink>
        </StructuredListCell>
        <StructuredListCell
            >{convertPercent(verbrauch.anteil[unit])}</StructuredListCell
        >
    </StructuredListRow>
{/each}
