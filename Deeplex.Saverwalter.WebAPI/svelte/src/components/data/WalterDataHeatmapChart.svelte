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
    import '@carbon/styles/css/styles.css';
    import '@carbon/charts-svelte/styles.css';
    import {
        HeatmapChart,
        type HeatmapChartOptions
    } from '@carbon/charts-svelte';
    import type { WalterDataConfigType, WalterDataPoint } from './WalterData';
    import { afterUpdate, onMount } from 'svelte';

    export let config: WalterDataConfigType;
    export let click:
        | ((e: CustomEvent<unknown>, config: WalterDataConfigType) => void)
        | undefined = undefined;
    $: options = config.options as HeatmapChartOptions;

    let heatMap: HeatmapChart;

    function getWohnungLinks(points: WalterDataPoint[]) {
        const map = new Map<string, string>();

        for (const point of points) {
            if (!point.group || !point.wohnungId || map.has(point.group)) {
                continue;
            }

            map.set(point.group, point.wohnungId);
        }

        return map;
    }

    function wrapTickLabelLink(
        label: SVGTextElement,
        href: string,
        marker: string
    ) {
        const currentParent = label.parentElement;

        if (currentParent?.tagName.toLowerCase() === 'a') {
            currentParent.setAttribute('href', href);
            return;
        }

        const tick = label.closest('g.tick');
        if (!tick) return;

        const existing = tick.querySelector(`a[${marker}]`);

        if (existing) {
            existing.setAttribute('href', href);
            return;
        }

        const anchor = document.createElementNS(
            'http://www.w3.org/2000/svg',
            'a'
        );
        anchor.setAttribute('href', href);
        anchor.setAttributeNS(
            'http://www.w3.org/1999/xlink',
            'xlink:href',
            href
        );
        anchor.setAttribute(marker, 'true');
        tick.insertBefore(anchor, label);
        anchor.appendChild(label);
    }

    function decorateAxisLabels() {
        if (!heatMap || !config?.data?.length) return;

        const links = getWohnungLinks(config.data);
        if (links.size === 0) return;

        const root = heatMap.$$.root as HTMLElement | undefined;
        const labels = root?.querySelectorAll<SVGTextElement>(
            'g.axis.left .tick text'
        );

        if (!labels?.length) return;

        for (const label of labels) {
            const key = label.textContent?.trim();
            if (!key) continue;

            const wohnungId = links.get(key);
            if (!wohnungId) continue;

            const href = `/wohnungen/${wohnungId}`;
            wrapTickLabelLink(label, href, 'data-walter-wohnung-link');
        }
    }

    onMount(() => {
        if (click) {
            heatMap.$$.root.addEventListener(
                'click',
                (e: CustomEvent<unknown>) => click!(e, config)
            );
        }

        decorateAxisLabels();
    });

    afterUpdate(() => {
        decorateAxisLabels();
    });
</script>

<HeatmapChart
    style="min-height: 20em !important; width: 100%;"
    bind:this={heatMap}
    bind:data={config.data}
    bind:options
/>
