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
    import '@carbon/styles/css/styles.css';
    import '@carbon/charts-svelte/styles.css';
    import { HeatmapChart } from '@carbon/charts-svelte';
    import type { WalterDataConfigType } from './WalterData';
    import { onMount } from 'svelte';

    export let config: WalterDataConfigType;
    export let click:
        | ((e: CustomEvent<unknown>, config: WalterDataConfigType) => void)
        | undefined = undefined;

    let heatMap: HeatmapChart;

    onMount(() => {
        if (click) {
            heatMap.$$.root.addEventListener(
                'click',
                (e: CustomEvent<unknown>) => click!(e, config)
            );
        }
    });
</script>

<HeatmapChart
    style="min-height: 20em !important; width: 100em;"
    bind:this={heatMap}
    bind:data={config.data}
    bind:options={config.options}
/>
