<script lang="ts">
    import { walter_goto } from '$walter/services/utils';
    import { Link } from 'carbon-components-svelte';

    let winWidth = 0;

    export let href: string;

    function click(e: Event) {
        e.preventDefault();
        walter_goto(href);
    }
</script>

<Link on:click={click} {href}><slot /></Link>
